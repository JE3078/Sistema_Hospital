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
    [Authorize(Roles = "1,2")]
    public class EnfermeroController : Controller
    {
        private readonly IEnfermeroService _enfermeroService;
        private readonly IBitacoraService _bitacoraService;
        private readonly HospitalContext _context;

        public EnfermeroController(IEnfermeroService enfermeroService, IBitacoraService bitacoraService, HospitalContext context)
        {
            _enfermeroService = enfermeroService;
            _bitacoraService = bitacoraService;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var enfermeros = await _enfermeroService.ListaEnfermeros();
            return View(enfermeros);
        }
        private async Task<List<SelectListItem>> ObtenerGenerosBD()
        {
            return await _context.Generos
                .Select(g => new SelectListItem { Value = g.IdGenero.ToString(), Text = g.Descripcion })
                .ToListAsync();
        }

        #region Crear 

        // GET: /Enfermero/Crear
        public async Task<IActionResult> Crear()
        {
            var model = new EnfermeroCrearVM
            {
                Generos = await ObtenerGenerosBD(),
            };
            return View(model);
        }

        // POST: /Enfermero/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(EnfermeroCrearVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await _enfermeroService.RegistrarEnfermero(model);
                if (exito)
                {
                    // Registro Obligatorio en la Bitácora del Sistema
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await _bitacoraService.RegistrarAccion(idAdmin,
                        $"CONTRATACIÓN: Registró al Enfermero(a) {model.Nombre} {model.Apellido} (DPI: {model.DPI}). " +
                        $"Se creó en paralelo su cuenta '{model.Username}' bajo transacción segura.");

                    return RedirectToAction("Index");
                }
                ViewBag.Error = "No se pudo registrar el enfermero. El Username o el DPI ya existen en la base de datos.";
            }

            model.Generos = await ObtenerGenerosBD();
            return View(model);
        }

        #endregion

        #region Editar

        // GET: /Enfermero/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var model = await _enfermeroService.ObtenerEnfermeroParaEditar(id);
            if (model == null)
            {
                return NotFound();
            }

            model.Generos = await ObtenerGenerosBD();
            return View(model);
        }

        // POST: /Enfermero/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EnfermeroEditarVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await _enfermeroService.ActualizarEnfermero(model);
                if (exito)
                {
                    // Auditoría obligatoria en Bitácora
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await _bitacoraService.RegistrarAccion(idAdmin,
                        $"MODIFICACIÓN: Actualizó el expediente del Enfermero ID #{model.IdEnfermero} ({model.Nombre} {model.Apellido}).");

                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Error = "No se pudo actualizar el registro. Verifique si el DPI ingresado está duplicado.";
            }

            model.Generos = await ObtenerGenerosBD();
            return View(model);
        }

        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var exito = await _enfermeroService.EliminarEnfermero(id);

            if (exito)
            {
                // Recuperamos datos saltando el filtro para registrar correctamente el nombre en la auditoría
                var enfermeroEliminado = await _context.Enfermeros
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(e => e.IdEnfermero == id);

                int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                await _bitacoraService.RegistrarAccion(idAdmin,
                    $"BORRADO LÓGICO: Inactivó el expediente del Enfermero ID #{id} " +
                    $"(Lic./Enf. {enfermeroEliminado?.Nombre} {enfermeroEliminado?.Apellido}) y suspendió su acceso al sistema.");

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "No se pudo dar de baja al enfermero.";
            return RedirectToAction(nameof(Index));
        }
    }
}
