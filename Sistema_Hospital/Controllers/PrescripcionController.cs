using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Sistema_Hospital.Controllers
{
    [Authorize]
    public class PrescripcionController : Controller
    {
        private readonly HospitalContext _context;

        public PrescripcionController(HospitalContext context)
        {
            _context = context;
        }

        // GET: /Prescripcion/ListaPorDiagnostico/5
        public async Task<IActionResult> ListaPorDiagnostico(int idDiagnostico)
        {
            var prescripciones = await _context.Prescripciones
                .Include(p => p.IdMedicamentoNavigation)
                .Where(p => p.IdDiagnostico == idDiagnostico)
                .Select(p => new PrescripcionListaVM
                {
                    IdPrescripcion = p.IdPrescripcion,
                    IdDiagnostico = p.IdDiagnostico,
                    MedicamentoNombre = p.IdMedicamentoNavigation.Nombre,
                    Dosis = p.Dosis,
                    Frecuencia = p.Frecuencia,
                    Duracion = p.Duracion,
                    Instrucciones = p.Instrucciones
                })
                .ToListAsync();

            return View(prescripciones);
        }
    }
}