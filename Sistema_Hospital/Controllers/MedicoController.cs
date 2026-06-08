using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models.ViewModels;
using Sistema_Hospital.Servicios;

namespace Sistema_Hospital.Controllers
{
    [Authorize(Roles = "1")] // Solo el Administrador puede contratar personal
    public class MedicoController : Controller
    {
        private readonly IMedicoService _medicoService;
        private readonly IBitacoraService _bitacoraService;
        private readonly HospitalContext _context;

        public MedicoController(IMedicoService medicoService, IBitacoraService bitacoraService, HospitalContext context)
        {
            _medicoService = medicoService;
            _bitacoraService = bitacoraService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Llama al servicio para obtener los datos de la vista de SQL
            var medicos = await _medicoService.ListaMedicos();

            // Envía la lista a la vista Views/Medico/Index.cshtml
            return View(medicos);
        }

        private async Task<List<SelectListItem>> ObtenerGenerosBD()
        {
            return await _context.Generos
                .Select(g => new SelectListItem { Value = g.IdGenero.ToString(), Text = g.Descripcion })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> ObtenerEspecialidadesBD()
        {
            return await _context.Especialidades
                .Select(e => new SelectListItem { Value = e.IdEspecialidad.ToString(), Text = e.Nombre })
                .ToListAsync();
        }

        #region Crear 

        // GET: /Medico/Crear
        public async Task<IActionResult> Crear()
        {
            var model = new MedicoCrearVM
            {
                Generos = await ObtenerGenerosBD(),
                Especialidades = await ObtenerEspecialidadesBD()
            };
            return View(model);
        }

        // POST: /Medico/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(MedicoCrearVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await _medicoService.RegistrarMedico(model);
                if (exito)
                {
                    // Registro obligatorio en Bitácora de Auditoría
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await _bitacoraService.RegistrarAccion(idAdmin,
                        $"CONTRATACIÓN: Registró al Dr. {model.Nombre} {model.Apellido} (Colegiado: {model.Colegiado}). " +
                        $"Se creó en paralelo su cuenta de acceso '{model.Username}' bajo una transacción ACID unificada.");

                    return RedirectToAction("Index", "Usuario"); // Redirecciona al listado general
                }
                ViewBag.Error = "No se pudo registrar el médico. El Username o el DPI ya existen en el sistema.";
            }

            model.Generos = await ObtenerGenerosBD();
            model.Especialidades = await ObtenerEspecialidadesBD();
            return View(model);
        }

        #endregion

        #region Editar
        // GET: /Medico/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var model = await _medicoService.ObtenerMedicoParaEditar(id);
            if (model == null)
            {
                return NotFound();
            }

            // Llenamos los catálogos desplegables desde la BD
            model.Generos = await _context.Generos.Select(g => new SelectListItem { Value = g.IdGenero.ToString(), Text = g.Descripcion }).ToListAsync();
            model.Especialidades = await _context.Especialidades.Select(e => new SelectListItem { Value = e.IdEspecialidad.ToString(), Text = e.Nombre }).ToListAsync();

            return View(model);
        }

        // POST: /Medico/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(MedicoEditarVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await _medicoService.ActualizarMedico(model);
                if (exito)
                {
                    // Auditoría obligatoria en Bitácora
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await _bitacoraService.RegistrarAccion(idAdmin, $"MODIFICACIÓN: Actualizó el expediente del Médico ID #{model.IdMedico} (Dr. {model.Nombre} {model.Apellido}).");

                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Error = "No se pudo actualizar el registro. Verifique que el DPI no esté duplicado.";
            }

            // Si algo falla, recargamos los Dropdowns antes de devolver la vista
            model.Generos = await _context.Generos.Select(g => new SelectListItem { Value = g.IdGenero.ToString(), Text = g.Descripcion }).ToListAsync();
            model.Especialidades = await _context.Especialidades.Select(e => new SelectListItem { Value = e.IdEspecialidad.ToString(), Text = e.Nombre }).ToListAsync();
            return View(model);
        }

        #endregion

        #region Eliminar

        // POST: /Medico/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var exito = await _medicoService.EliminarMedico(id);

            if (exito)
            {
                // Obtener datos del médico (saltándose el filtro de activos para la bitácora)
                var medicoEliminado = await _context.Medicos
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(m => m.IdMedico == id);

                // Auditoría obligatoria en Bitácora
                int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                await _bitacoraService.RegistrarAccion(idAdmin,
                    $"BORRADO LÓGICO: Inactivó el expediente del Médico ID #{id} " +
                    $"(Dr. {medicoEliminado?.Nombre} {medicoEliminado?.Apellido}) y suspendió su cuenta de acceso.");

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "No se pudo eliminar el registro del médico.";
            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}