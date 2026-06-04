using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;

namespace Sistema_Hospital.Servicios
{
    public interface ICitaService
    {
        Task<bool> ActualizarCita(CitaEditarVM model);
        Task<IEnumerable<CitaListaVM>> ListaCitas();
        Task<IEnumerable<CitaListaVM>> ListaCitasPorMedico(int idMedico);
        Task<CitaEditarVM?> ObtenerCitaParaEditar(int idCita);
        Task<CitaCrearVM> ObtenerDatosCrear();
        Task<bool> RegistrarCita(CitaCrearVM model);
    }
    public class CitaService : ICitaService
    {
        private readonly HospitalContext context;

        public CitaService(HospitalContext context)
        {
            this.context = context;
        }
        
        public async Task<IEnumerable<CitaListaVM>> ListaCitas()
        {
            return await context.Cita
                            .Include(c => c.IdMedicoNavigation)
                            .Include(c => c.IdPacienteNavigation)
                            .Include(c => c.IdEstadoNavigation)
                            .Select(c => new CitaListaVM
                            {
                                IdCita = c.IdCita,
                                Motivo = c.Motivo,
                                FechaHora = c.FechaHora,
                                NombreMedico = c.IdMedicoNavigation.Nombre + " " +c.IdMedicoNavigation.Apellido, 
                                NombrePaciente = c.IdPacienteNavigation.Nombre + " " + c.IdPacienteNavigation.Apellido, 
                                Estado = c.IdEstadoNavigation.NombreEstado 
                            })
                            .ToListAsync();
        }

        public async Task<IEnumerable<CitaListaVM>> ListaCitasPorMedico(int idMedico)
        {
            return await context.Cita
                .Where(c => c.IdMedico == idMedico) // Filtramos por el ID del médico recibido
                .Select(c => new CitaListaVM
                {
                    IdCita = c.IdCita,
                    Motivo = c.Motivo,
                    FechaHora = c.FechaHora,
                    NombreMedico = c.IdMedicoNavigation.Nombre + " " + c.IdMedicoNavigation.Apellido,
                    NombrePaciente = c.IdPacienteNavigation.Nombre + " " + c.IdPacienteNavigation.Apellido,
                    Estado = c.IdEstadoNavigation.NombreEstado
                })
                .ToListAsync();
        }

        public async Task<CitaCrearVM> ObtenerDatosCrear()
        {
            return new CitaCrearVM
            {
                Pacientes = await ObtenerListaPacientes(),
                Medicos = await ObtenerListaMedicos(),
                Estados = await ObtenerListaEstados(),
                Prescripcion = new PrescripcionCrearVM
                {
                    Medicamentos = await ObtenerListaMedicamentos(),
                }
            };
        }
        public async Task<bool> RegistrarCita(CitaCrearVM model)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // A. Cita
                var nuevaCita = new Cita
                {
                    Motivo = model.Motivo,
                    FechaHora = model.FechaHora,
                    IdPaciente = model.IdPaciente,
                    IdMedico = model.IdMedico,
                    IdEstado = model.IdEstado
                };
                context.Cita.Add(nuevaCita);
                await context.SaveChangesAsync();

                // B. Diagnóstico
                if (model.Diagnostico != null && !string.IsNullOrWhiteSpace(model.Diagnostico.Descripcion))
                {
                    var nuevoDiagnostico = new Diagnostico
                    {
                        IdCita = nuevaCita.IdCita,
                        Descripcion = model.Diagnostico.Descripcion,
                        Observaciones = model.Diagnostico.Observaciones
                    };
                    context.Diagnosticos.Add(nuevoDiagnostico);
                    await context.SaveChangesAsync(); // Genera ID_Diagnostico

                    // C. Prescripción (Requiere que exista el diagnóstico)
                    if (model.Prescripcion != null && model.Prescripcion.IdMedicamento > 0)
                    {
                        var nuevaPrescripcion = new Prescripcion
                        {
                            IdDiagnostico = nuevoDiagnostico.IdDiagnostico,
                            IdMedicamento = model.Prescripcion.IdMedicamento,
                            Dosis = model.Prescripcion.Dosis,
                            Frecuencia = model.Prescripcion.Frecuencia,
                            Duracion = model.Prescripcion.Duracion,
                            Instrucciones = model.Prescripcion.Instrucciones
                        };
                        context.Prescripciones.Add(nuevaPrescripcion);
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<CitaEditarVM?> ObtenerCitaParaEditar(int idCita)
        {
            var cita = await context.Cita
                .Include(c => c.Diagnosticos)
                    .ThenInclude(d => d.Prescripcions) // Incluye la receta guardada
                .FirstOrDefaultAsync(c => c.IdCita == idCita);

            if (cita == null) return null;

            var diagnosticoExistente = cita.Diagnosticos.FirstOrDefault();
            var prescripcionExistente = diagnosticoExistente?.Prescripcions.FirstOrDefault();
            var listaMedicamentos = await ObtenerListaMedicamentos();

            return new CitaEditarVM
            {
                IdCita = cita.IdCita,
                Motivo = cita.Motivo,
                FechaHora = cita.FechaHora,
                IdMedico = cita.IdMedico,
                IdPaciente = cita.IdPaciente,
                IdEstado = cita.IdEstado,
                Pacientes = await ObtenerListaPacientes(),
                Medicos = await ObtenerListaMedicos(),
                Estados = await ObtenerListaEstados(),

                Diagnostico = diagnosticoExistente != null ? new DiagnosticoEditarVM
                {
                    IdDiagnostico = diagnosticoExistente.IdDiagnostico,
                    IdCita = diagnosticoExistente.IdCita,
                    Descripcion = diagnosticoExistente.Descripcion,
                    Observaciones = diagnosticoExistente.Observaciones
                } : new DiagnosticoEditarVM { IdDiagnostico = 0, IdCita = cita.IdCita, Descripcion = "", Observaciones = "" },

                Prescripcion = prescripcionExistente != null ? new PrescripcionEditarVM
                {
                    IdPrescripcion = prescripcionExistente.IdPrescripcion,
                    IdDiagnostico = prescripcionExistente.IdDiagnostico,
                    IdMedicamento = prescripcionExistente.IdMedicamento,
                    Dosis = prescripcionExistente.Dosis,
                    Frecuencia = prescripcionExistente.Frecuencia,
                    Duracion = prescripcionExistente.Duracion,
                    Instrucciones = prescripcionExistente.Instrucciones,
                    Medicamentos = listaMedicamentos
                } : new PrescripcionEditarVM { IdPrescripcion = 0, IdDiagnostico = diagnosticoExistente?.IdDiagnostico ?? 0, Medicamentos = listaMedicamentos }
            };
        }

        public async Task<bool> ActualizarCita(CitaEditarVM model)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cita
                var cita = await context.Cita.FirstOrDefaultAsync(c => c.IdCita == model.IdCita);
                if (cita == null) return false;

                cita.IdPaciente = model.IdPaciente;
                cita.IdMedico = model.IdMedico;
                cita.IdEstado = model.IdEstado;
                cita.FechaHora = model.FechaHora;
                cita.Motivo = model.Motivo;
                context.Cita.Update(cita);

                // 2. Diagnóstico
                if (model.Diagnostico != null && !string.IsNullOrWhiteSpace(model.Diagnostico.Descripcion))
                {
                    var diagnosticoExistente = await context.Diagnosticos
                        .FirstOrDefaultAsync(d => d.IdCita == model.IdCita);

                    int idDiagnosticoActual = 0;

                    if (diagnosticoExistente != null)
                    {
                        diagnosticoExistente.Descripcion = model.Diagnostico.Descripcion;
                        diagnosticoExistente.Observaciones = model.Diagnostico.Observaciones;
                        context.Diagnosticos.Update(diagnosticoExistente);
                        await context.SaveChangesAsync();
                        idDiagnosticoActual = diagnosticoExistente.IdDiagnostico;
                    }
                    else
                    {
                        var nuevoDiagnostico = new Diagnostico
                        {
                            IdCita = model.IdCita,
                            Descripcion = model.Diagnostico.Descripcion,
                            Observaciones = model.Diagnostico.Observaciones
                        };
                        context.Diagnosticos.Add(nuevoDiagnostico);
                        await context.SaveChangesAsync(); // Guardado intermedio para obtener el ID
                        idDiagnosticoActual = nuevoDiagnostico.IdDiagnostico;
                    }

                    // 3. Prescripción
                    if (model.Prescripcion != null && model.Prescripcion.IdMedicamento > 0)
                    {
                        var prescripcionExistente = await context.Prescripciones
                            .FirstOrDefaultAsync(p => p.IdDiagnostico == idDiagnosticoActual);

                        if (prescripcionExistente != null)
                        {
                            // YA EXISTÍA RECETA: Actualizamos
                            prescripcionExistente.IdMedicamento = model.Prescripcion.IdMedicamento;
                            prescripcionExistente.Dosis = model.Prescripcion.Dosis;
                            prescripcionExistente.Frecuencia = model.Prescripcion.Frecuencia;
                            prescripcionExistente.Duracion = model.Prescripcion.Duracion;
                            prescripcionExistente.Instrucciones = model.Prescripcion.Instrucciones;
                            context.Prescripciones.Update(prescripcionExistente);
                        }
                        else
                        {
                            // NUEVA RECETA: Insertamos
                            var nuevaPrescripcion = new Prescripcion
                            {
                                IdDiagnostico = idDiagnosticoActual,
                                IdMedicamento = model.Prescripcion.IdMedicamento,
                                Dosis = model.Prescripcion.Dosis,
                                Frecuencia = model.Prescripcion.Frecuencia,
                                Duracion = model.Prescripcion.Duracion,
                                Instrucciones = model.Prescripcion.Instrucciones
                            };
                            context.Prescripciones.Add(nuevaPrescripcion);
                        }
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Métodos auxiliares privados para rellenar los DropDownLists
        private async Task<List<SelectListItem>> ObtenerListaPacientes() =>
            await context.Pacientes.Select(p => new SelectListItem { Value = p.IdPaciente.ToString(), Text = p.Nombre }).ToListAsync();

        private async Task<List<SelectListItem>> ObtenerListaMedicos() =>
            await context.Medicos.Select(m => new SelectListItem { Value = m.IdMedico.ToString(), Text = m.Nombre }).ToListAsync();

        private async Task<List<SelectListItem>> ObtenerListaEstados() =>
            await context.Estados.Select(e => new SelectListItem { Value = e.IdEstado.ToString(), Text = e.NombreEstado }).ToListAsync();

        private async Task<List<SelectListItem>> ObtenerListaMedicamentos() =>
        await context.Medicamentos
            .Select(m => new SelectListItem { Value = m.IdMedicamento.ToString(), Text = m.Nombre })
            .ToListAsync();
    }
}
