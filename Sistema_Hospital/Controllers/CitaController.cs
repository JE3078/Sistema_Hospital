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
    [Authorize]
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

            if(rolClaim == "1")
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
            if (ModelState.IsValid)
            {
                var exito = await citaService.RegistrarCita(model);
                if (exito)
                {
                    // Registro obligatorio en Bitácora de Auditoría
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await bitacoraService.RegistrarAccion(idAdmin, $"REGISTRO: Se agendo cita para el paciente {model.IdPaciente} atendido por el doctor/doctora {model.IdMedico}");

                    return RedirectToAction("Index", "Cita"); // Redirecciona al listado general
                }
                ViewBag.Error = "No se pudo registrar la cita.";
            }

            var datos = await citaService.ObtenerDatosCrear();

            model.Pacientes = datos.Pacientes;
            model.Medicos = datos.Medicos;
            model.Estados = datos.Estados;

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

        // POST: /Cita/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(CitaEditarVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await citaService.ActualizarCita(model);
                if (exito)
                {
                    // Auditoría obligatoria en Bitácora
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await bitacoraService.RegistrarAccion(idAdmin, $"MODIFICACIÓN: Actualizó los datos de la cita: ID #{model.IdCita}.");

                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Error = "No se pudo actualizar el registro";
            }

            return View(model);
        }
        #endregion
    }
}
