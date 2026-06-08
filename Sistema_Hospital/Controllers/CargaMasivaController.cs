using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema_Hospital.Servicios;

namespace Sistema_Hospital.Controllers
{
    [Authorize(Roles = "1,2,3")]
    public class CargaMasivaController : Controller
    {
        private readonly ICargaMasivaService _cargaMasivaService;
        private readonly IBitacoraService _bitacoraService;

        public CargaMasivaController(ICargaMasivaService cargaMasivaService, IBitacoraService bitacoraService)
        {
            _cargaMasivaService = cargaMasivaService;
            _bitacoraService = bitacoraService;
        }

        // GET: /CargaMasiva
        public IActionResult Index()
        {
            return View();
        }

        // POST: /CargaMasiva/CargarMedicamentos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CargarMedicamentos(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Debes seleccionar un archivo Excel (.xlsx).";
                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(archivo.FileName).ToLower();
            if (extension != ".xlsx")
            {
                TempData["Error"] = "Solo se aceptan archivos con formato .xlsx";
                return RedirectToAction(nameof(Index));
            }

            using var stream = archivo.OpenReadStream();
            var resultado = await _cargaMasivaService.CargarMedicamentosDesdeExcel(stream);

            // Auditoría obligatoria
            int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
            await _bitacoraService.RegistrarAccion(idAdmin,
                $"CARGA MASIVA: Se cargaron {resultado.RegistrosCargados} medicamentos desde Excel. " +
                $"Omitidos: {resultado.RegistrosOmitidos}.");

            TempData["RegistrosCargados"] = resultado.RegistrosCargados;
            TempData["RegistrosOmitidos"] = resultado.RegistrosOmitidos;
            TempData["Errores"] = string.Join("|", resultado.Errores);

            return RedirectToAction(nameof(Index));
        }
    }
}