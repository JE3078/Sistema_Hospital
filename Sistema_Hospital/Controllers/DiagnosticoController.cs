using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema_Hospital.Models.ViewModels;
using Sistema_Hospital.Servicios;

namespace Sistema_Hospital.Controllers
{
    [Authorize]
    public class DiagnosticoController : Controller
    {
        private readonly IDiagnosticoService _diagnosticoService;
        private readonly IBitacoraService _bitacoraService;

        public DiagnosticoController(IDiagnosticoService diagnosticoService, IBitacoraService bitacoraService)
        {
            _diagnosticoService = diagnosticoService;
            _bitacoraService = bitacoraService;
        }

        #region Crear
        // GET: /Diagnostico/Crear?idCita=5
        public async Task<IActionResult> Crear(int idCita)
        {
            var model = await _diagnosticoService.ObtenerDatosParaCrear(idCita);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DiagnosticoCrearVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await _diagnosticoService.RegistrarDiagnostico(model);
                if (exito)
                {
                    // Auditoría en Bitácora
                    int idUsuario = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await _bitacoraService.RegistrarAccion(idUsuario, $"DIAGNÓSTICO: Se registró diagnóstico para la cita #{model.IdCita}.");

                    // Redirecciona al listado de citas
                    return RedirectToAction("Index", "Cita");
                }
                ViewBag.Error = "No se pudo guardar el diagnóstico.";
            }
            return View(model);
        }
        #endregion

        #region Editar
        // GET: /Diagnostico/Editar?idCita=5
        // Buscamos por idCita para facilitar la navegación desde el módulo de Citas
        public async Task<IActionResult> Editar(int idCita)
        {
            var model = await _diagnosticoService.ObtenerDiagnosticoParaEditar(idCita);
            if (model == null)
            {
                // Si no existe diagnóstico, lo mandamos a crear uno
                return RedirectToAction(nameof(Crear), new { idCita = idCita });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(DiagnosticoEditarVM model)
        {
            if (ModelState.IsValid)
            {
                var exito = await _diagnosticoService.ActualizarDiagnostico(model);
                if (exito)
                {
                    // Auditoría en Bitácora
                    int idUsuario = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
                    await _bitacoraService.RegistrarAccion(idUsuario, $"DIAGNÓSTICO: Se modificó el diagnóstico ID #{model.IdDiagnostico} de la cita #{model.IdCita}.");

                    return RedirectToAction("Index", "Cita");
                }
                ViewBag.Error = "No se pudo actualizar el diagnóstico.";
            }
            return View(model);
        }
        #endregion
    }
}