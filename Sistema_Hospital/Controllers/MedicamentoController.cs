using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;
using Sistema_Hospital.Servicios;

namespace Sistema_Hospital.Controllers
{
    [Authorize(Roles = "1,2,3")]
    public class MedicamentoController : Controller
    {
        private readonly IMedicamentoService medicamentoService;
        private readonly IBitacoraService bitacoraService;
        private readonly HospitalContext context;

        public MedicamentoController(IMedicamentoService medicamentoService, IBitacoraService bitacoraService, HospitalContext context)
        {
            this.medicamentoService = medicamentoService;
            this.bitacoraService = bitacoraService;
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            var medicamentos = await medicamentoService.ListaMedicamentos();
            return View(medicamentos);
        }

        #region Crear

        // GET: /Medicamento/Crear
        public async Task<IActionResult> Crear()
        {
            var model = new Medicamento
            {
                    
            };

            return View(model);
        }

        // POST: /Medicamento/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Medicamento model)
        {
            if (ModelState.IsValid)
            {
                var exito = await medicamentoService.RegistrarMedicamento(model);
                if (exito)
                {
                    // Registro obligatorio en Bitácora de Auditoría
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await bitacoraService.RegistrarAccion(idAdmin, $"REGISTRO: Registro el siguiente medicamento en el sistema: {model.Nombre} Inventario Inicial: {model.Stock} ");

                    return RedirectToAction("Index", "Usuario"); // Redirecciona al listado general
                }
                ViewBag.Error = "No se pudo registrar el paciente. El DPI o eel correo ya existee en el sistema.";
            }

            return View(model);
        }

        #endregion

        #region Editar
        // GET: /Medicamento/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var model = await medicamentoService.ObtenerMedicamentoParaEditar(id);
            if (model == null)
            {
                return NotFound();
            }

            // Llenamos los catálogos desplegables desde la BD

            return View(model);
        }

        // POST: /Medicamento/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Medicamento model)
        {
            if (ModelState.IsValid)
            {
                var exito = await medicamentoService.ActualizarMedicamento(model);
                if (exito)
                {
                    // Auditoría obligatoria en Bitácora
                    int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await bitacoraService.RegistrarAccion(idAdmin, $"MODIFICACIÓN: Actualizó los datos del medicamento: ID #{model.IdMedicamento} ({model.Nombre} {model.Precio}).");

                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Error = "No se pudo actualizar el registro. Verifique que el DPI no esté duplicado.";
            }

            return View(model);
        }
        #endregion

    }
}
