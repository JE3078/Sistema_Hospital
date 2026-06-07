using Microsoft.AspNetCore.Mvc;
using Sistema_Hospital.Models.ViewModels;
using Sistema_Hospital.Servicios;
using System.Threading.Tasks;

namespace Sistema_Hospital.Controllers
{
    public class IngresoController : Controller
    {
        private readonly IIngresoService _ingresoService;

        public IngresoController(IIngresoService ingresoService)
        {
            _ingresoService = ingresoService;
        }

        // GET: Ingreso
        public async Task<IActionResult> Index()
        {
            var ingresos = await _ingresoService.ListaIngresos();
            return View(ingresos);
        }

        // GET: Ingreso/Crear
        public async Task<IActionResult> Crear()
        {
            var model = await _ingresoService.PrepararListasDesplegables();
            return View(model);
        }

        // POST: Ingreso/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(IngresoCrearVM model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Reemplazar el '1' por el ID dinámico del usuario autenticado si usas Claims (ej: User.FindFirst...)
                int idUsuarioActual = 1;

                var exito = await _ingresoService.RegistrarIngreso(model, idUsuarioActual);
                if (exito)
                {
                    TempData["SuccessMessage"] = "El ingreso hospitalario se ha registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Ocurrió un error al procesar la transacción en la base de datos.");
            }

            // Si falla la validación, volvemos a cargar los select items antes de retornar la vista
            var listasRehidratadas = await _ingresoService.PrepararListasDesplegables();
            model.Pacientes = listasRehidratadas.Pacientes;
            model.Areas = listasRehidratadas.Areas;
            model.Estados = listasRehidratadas.Estados;

            return View(model);
        }

        // POST: Ingreso/DarDeAlta/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DarDeAlta(int id)
        {
            // TODO: Reemplazar el '1' por el ID dinámico del usuario autenticado si usas Claims
            int idUsuarioActual = 1;

            var exito = await _ingresoService.DarDeAlta(id, idUsuarioActual);

            if (exito)
            {
                TempData["SuccessMessage"] = "El paciente ha sido dado de alta exitosamente y se registró su fecha de salida.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo procesar el alta del paciente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}