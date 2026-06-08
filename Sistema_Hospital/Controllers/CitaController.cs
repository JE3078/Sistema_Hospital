using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;
using Sistema_Hospital.Servicios;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sistema_Hospital.Controllers
{
    [Authorize(Roles = "1,2,3")]
    public class CitaController : Controller
    {
        private readonly ICitaService citaService;
        private readonly IBitacoraService bitacoraService;
        private readonly HospitalContext context;

        public CitaController(ICitaService citaService, IBitacoraService bitacoraService, HospitalContext context)
        {
            this.citaService = citaService;
            this.bitacoraService = bitacoraService;
            this.context = context;
        }
        public async Task<IActionResult> Index()
        {
            var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rolClaim == "1" || rolClaim == "3")
            {
                var citas = await citaService.ListaCitas();
                return View(citas);
            }
            else if (rolClaim == "2") // Médico
            {
                // 1. Buscamos el ID_Usuario que sí existe en tu sesión actual
                var idUsuarioClaim = User.FindFirst("ID_Usuario")?.Value;

                if (int.TryParse(idUsuarioClaim, out int idUsuario))
                {
                    // 2. Buscamos en la base de datos el ID del médico que está ligado a este ID_Usuario
                    // Ajusta "IdUsuario" al nombre exacto de la propiedad en tu tabla Medico
                    var medico = await context.Medicos
                        .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);

                    if (medico != null)
                    {
                        // 3. Pasamos el verdadero ID del médico al servicio
                        var citasMedico = await citaService.ListaCitasPorMedico(medico.IdMedico);
                        return View(citasMedico);
                    }
                }

                return Forbid();
            }

            return Forbid();
        }

        #region Crear
        public async Task<IActionResult> Crear()
        {
            var model = await citaService.ObtenerDatosCrear();
            return View(model);
        }

        // POST: /Cita/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CitaCrearVM model)
        {
            // 1. CONTROL DEL DIAGNÓSTICO OPCIONAL
            // Si la descripción viene vacía o nula, asumimos que no se ingresó diagnóstico en la creación
            if (model.Diagnostico == null || string.IsNullOrWhiteSpace(model.Diagnostico.Descripcion))
            {
                ModelState.Remove("Diagnostico.Descripcion");
                ModelState.Remove("Diagnostico.Observaciones");

                // Lo seteamos en nulo para que el servicio sepa que no debe insertar en la tabla Diagnostico
                model.Diagnostico = null;
            }

            // 2. CONTROL DE LA PRESCRIPCIÓN OPCIONAL
            // Si no hay diagnóstico, o si no se seleccionó un medicamento válido (IdMedicamento es 0 o null)
            if (model.Prescripcion == null || model.Prescripcion.IdMedicamento == 0 || model.Diagnostico == null)
            {
                ModelState.Remove("Prescripcion.IdMedicamento");
                ModelState.Remove("Prescripcion.Dosis");
                ModelState.Remove("Prescripcion.Frecuencia");
                ModelState.Remove("Prescripcion.Duracion");

                // Lo seteamos en nulo para que el servicio no intente insertar una prescripción huérfana
                model.Prescripcion = null;
            }

            // Ahora el ModelState estará limpio y dará TRUE si los datos básicos de la Cita están correctos
            if (ModelState.IsValid)
            {
                var exito = await citaService.RegistrarCita(model);
                if (exito)
                {
                    // Registro obligatorio en Bitácora de Auditoría
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await bitacoraService.RegistrarAccion(idAdmin, $"REGISTRO: Se agendó cita para el paciente {model.IdPaciente} con el médico {model.IdMedico}");

                    return RedirectToAction("Index", "Cita");
                }
                ViewBag.Error = "No se pudo registrar la cita de forma transaccional.";
            }

            // Si algo falla, recargamos los catálogos para no romper los dropdowns de la vista
            var datos = await citaService.ObtenerDatosCrear();
            model.Pacientes = datos.Pacientes;
            model.Medicos = datos.Medicos;
            model.Estados = datos.Estados;

            if (model.Diagnostico == null) model.Diagnostico = datos.Diagnostico;
            if (model.Prescripcion == null) model.Prescripcion = datos.Prescripcion;

            return View(model);
        }

        #endregion

        #region Editar
        // GET: /Cita/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var model = await citaService.ObtenerCitaParaEditar(id);
            if (model == null)
            {
                return NotFound();
            }


            return View(model);
        }

        // POST: /Cita/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(CitaEditarVM model)
        {
            // 1. Si el diagnóstico viene vacío, limpiamos sus validaciones por si era una cita limpia
            if (model.Diagnostico == null || string.IsNullOrWhiteSpace(model.Diagnostico.Descripcion))
            {
                ModelState.Remove("Diagnostico.Descripcion");
                ModelState.Remove("Diagnostico.Observaciones");
            }

            // 2. Si el modelo pasa las validaciones (incluyendo las del modal que ahora son obligatorias si se interactúa)
            if (ModelState.IsValid)
            {
                var exito = await citaService.ActualizarCita(model);
                if (exito)
                {
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await bitacoraService.RegistrarAccion(idAdmin, $"MODIFICACIÓN: Se actualizaron los datos clínicos de la cita ID #{model.IdCita}.");

                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Error = "No se pudieron guardar los cambios en la base de datos.";
            }

            // ====================================================================
            // SOLUCIÓN CLAVE: Volver a rellenar TODOS los catálogos si el modelo es inválido
            // ====================================================================
            // Usamos el servicio existente para traer una estructura limpia con los catálogos llenos de la BD
            var datosCatalogo = await citaService.ObtenerCitaParaEditar(model.IdCita);

            if (datosCatalogo != null)
            {
                // Rellenamos las listas de la Cita principal para que no aparezcan vacías
                model.Pacientes = datosCatalogo.Pacientes;
                model.Medicos = datosCatalogo.Medicos;
                model.Estados = datosCatalogo.Estados;

                // Rellenamos el catálogo de medicamentos dentro del objeto Prescripcion del modelo
                if (model.Prescripcion == null)
                {
                    // Si por algún motivo llegó nulo, lo inicializamos con su catálogo correspondiente
                    model.Prescripcion = new PrescripcionEditarVM
                    {
                        IdDiagnostico = model.Diagnostico?.IdDiagnostico ?? 0,
                        Medicamentos = datosCatalogo.Prescripcion?.Medicamentos
                    };
                }
                else
                {
                    // Si el objeto ya existía (con los errores del usuario), solo le reinyectamos la lista de medicamentos
                    model.Prescripcion.Medicamentos = datosCatalogo.Prescripcion?.Medicamentos;
                }
            }
            // Detectar si existen errores de validación relacionados con Prescripción
            bool hayErroresPrescripcion = ModelState
                .Where(x => x.Key.StartsWith("Prescripcion") && x.Value.Errors.Count > 0)
                .Any();

            if (hayErroresPrescripcion)
            {
                ViewBag.AbrirModalPrescripcion = true;
            }

            // Devolvemos el modelo completamente rehidratado a la vista
            return View(model);
        }
        #endregion
    }
}
