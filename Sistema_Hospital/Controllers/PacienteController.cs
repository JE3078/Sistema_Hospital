using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models.ViewModels;
using Sistema_Hospital.Servicios;
using System.Threading.Tasks;

namespace Sistema_Hospital.Controllers
{
    //[Authorize(Roles = "1, 2")] // Solo el Administrador puede contratar personal
//prueba
    public class PacienteController : Controller
    {
        private readonly IPacienteService pacienteService;
        private readonly IBitacoraService bitacoraService;
        private readonly HospitalContext context;

        public PacienteController(IPacienteService pacienteService, IBitacoraService bitacoraService, HospitalContext context)
        {
            this.pacienteService = pacienteService;
            this.bitacoraService = bitacoraService;
            this.context = context;
        }
        public async Task<IActionResult> Index()
        {
            var pacientes = await pacienteService.ListaPacientes();
            return View(pacientes);
        }
        private async Task<List<SelectListItem>> ObtenerGenerosBD()
        {
            return await context.Generos
                .Select(g => new SelectListItem { Value = g.IdGenero.ToString(), Text = g.Descripcion })
                .ToListAsync();
        }

        #region Crear

        // GET: /Paciente/Crear
        public async Task<IActionResult> Crear()
        {
            var model = new PacienteCrearVM
            {
                Generos = await ObtenerGenerosBD(),
            };
            return View(model);
        }

        // POST: /Paciente/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PacienteCrearVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await pacienteService.RegistrarPaciente(model);
                if (exito)
                {
                    // Registro obligatorio en Bitácora de Auditoría
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await bitacoraService.RegistrarAccion(idAdmin, $"REGISTRO: Registró al Paciente {model.Nombre} {model.Apellido} ");

                    return RedirectToAction("Index", "Usuario"); // Redirecciona al listado general
                }
                ViewBag.Error = "No se pudo registrar el paciente. El DPI o eel correo ya existee en el sistema.";
            }

            model.Generos = await ObtenerGenerosBD();
            return View(model);
        }

        #endregion


        #region Editar
        // GET: /Paciente/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var model = await pacienteService.ObtenerPacienteParaEditar(id);
            if (model == null)
            {
                return NotFound();
            }

            // Llenamos los catálogos desplegables desde la BD
            model.Generos = await context.Generos.Select(g => new SelectListItem { Value = g.IdGenero.ToString(), Text = g.Descripcion }).ToListAsync();

            return View(model);
        }

        // POST: /Paciente/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(PacienteEditarVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await pacienteService.ActualizarPaciente(model);
                if (exito)
                {
                    // Auditoría obligatoria en Bitácora
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await bitacoraService.RegistrarAccion(idAdmin, $"MODIFICACIÓN: Actualizó el expediente del Paciente ID #{model.IdPaciente} ({model.Nombre} {model.Apellido}).");

                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Error = "No se pudo actualizar el registro. Verifique que el DPI no esté duplicado.";
            }

            // Si algo falla, recargamos los Dropdowns antes de devolver la vista
            model.Generos = await context.Generos.Select(g => new SelectListItem { Value = g.IdGenero.ToString(), Text = g.Descripcion }).ToListAsync();
            return View(model);
        }
        #endregion
    }
}
