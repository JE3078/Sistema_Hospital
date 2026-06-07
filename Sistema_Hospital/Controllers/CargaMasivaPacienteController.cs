using Microsoft.AspNetCore.Mvc;
using Sistema_Hospital.Servicios;

namespace Sistema_Hospital.Controllers
{
    public class CargaMasivaPacienteController : Controller
    {
        private readonly ICargaMasivaPacienteService _cargaService;
        private readonly IBitacoraService _bitacoraService;

        public CargaMasivaPacienteController(ICargaMasivaPacienteService cargaService, IBitacoraService bitacoraService)
        {
            _cargaService = cargaService;
            _bitacoraService = bitacoraService;
        }

        // GET: /CargaMasivaPaciente
        public IActionResult Index()
        {
            return View();
        }

        // POST: /CargaMasivaPaciente/CargarPacientes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CargarPacientes(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Debes seleccionar un archivo CSV.";
                return RedirectToAction(nameof(Index));
            }

            if (Path.GetExtension(archivo.FileName).ToLower() != ".csv")
            {
                TempData["Error"] = "Solo se aceptan archivos con formato .csv";
                return RedirectToAction(nameof(Index));
            }

            using var stream = archivo.OpenReadStream();
            var resultado = await _cargaService.CargarPacientesDesdeCSV(stream);

            // Auditoría obligatoria en Bitácora
            int idAdmin = int.Parse(User.FindFirst("ID_Usuario")?.Value ?? "0");
            await _bitacoraService.RegistrarAccion(idAdmin,
                $"CARGA MASIVA: Se cargaron {resultado.RegistrosCargados} pacientes desde CSV. " +
                $"Omitidos: {resultado.RegistrosOmitidos}.");

            TempData["RegistrosCargados"] = resultado.RegistrosCargados;
            TempData["RegistrosOmitidos"] = resultado.RegistrosOmitidos;
            TempData["Errores"] = string.Join("|", resultado.Errores);

            return RedirectToAction(nameof(Index));
        }
    }
}