using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;

namespace Sistema_Hospital.Servicios
{
    public interface IDiagnosticoService
    {
        Task<bool> ActualizarDiagnostico(DiagnosticoEditarVM model);
        Task<DiagnosticoCrearVM?> ObtenerDatosParaCrear(int idCita);
        Task<DiagnosticoEditarVM?> ObtenerDiagnosticoParaEditar(int idCita);
        Task<bool> RegistrarDiagnostico(DiagnosticoCrearVM model);
    }
    public class DiagnosticoService : IDiagnosticoService
    {
        private readonly HospitalContext context;

        public DiagnosticoService(HospitalContext context)
        {
            this.context = context;
        }

        // Carga los datos informativos de la cita para el formulario de creación
        public async Task<DiagnosticoCrearVM?> ObtenerDatosParaCrear(int idCita)
        {
            var cita = await context.Cita
                .Include(c => c.IdPacienteNavigation)
                .FirstOrDefaultAsync(c => c.IdCita == idCita);

            if (cita == null) return null;

            return new DiagnosticoCrearVM
            {
                IdCita = cita.IdCita,
                PacienteNombre = $"{cita.IdPacienteNavigation.Nombre} {cita.IdPacienteNavigation.Apellido}",
                FechaCita = cita.FechaHora.ToString("dd/MM/yyyy HH:mm")
            };
        }

        public async Task<bool> RegistrarDiagnostico(DiagnosticoCrearVM model)
        {
            try
            {
                var diagnostico = new Diagnostico
                {
                    IdCita = model.IdCita,
                    Descripcion = model.Descripcion,
                    Observaciones = model.Observaciones
                };

                context.Diagnosticos.Add(diagnostico);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Busca el diagnóstico existente usando el ID de la Cita
        public async Task<DiagnosticoEditarVM?> ObtenerDiagnosticoParaEditar(int idCita)
        {
            var diagnostico = await context.Diagnosticos
                .Include(d => d.IdCitaNavigation)
                .ThenInclude(c => c.IdPacienteNavigation)
                .FirstOrDefaultAsync(d => d.IdCita == idCita);

            if (diagnostico == null) return null;

            return new DiagnosticoEditarVM
            {
                IdDiagnostico = diagnostico.IdDiagnostico,
                IdCita = diagnostico.IdCita,
                PacienteNombre = $"{diagnostico.IdCitaNavigation.IdPacienteNavigation.Nombre} {diagnostico.IdCitaNavigation.IdPacienteNavigation.Apellido}",
                Descripcion = diagnostico.Descripcion,
                Observaciones = diagnostico.Observaciones
            };
        }

        public async Task<bool> ActualizarDiagnostico(DiagnosticoEditarVM model)
        {
            try
            {
                var diagnostico = await context.Diagnosticos.FindAsync(model.IdDiagnostico);
                if (diagnostico == null) return false;

                diagnostico.Descripcion = model.Descripcion;
                diagnostico.Observaciones = model.Observaciones;

                context.Diagnosticos.Update(diagnostico);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
