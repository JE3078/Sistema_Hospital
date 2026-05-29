using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;

namespace Sistema_Hospital.Servicios
{
    public interface IPacienteService
    {
        Task<bool> ActualizarPaciente(PacienteEditarVM model);
        Task<IEnumerable<PacienteListaVM>> ListaPacientes();
        Task<PacienteEditarVM> ObtenerPacienteParaEditar(int idPaciente);
        Task<bool> RegistrarPaciente(PacienteCrearVM model);
    }
    public class PacienteService : IPacienteService
    {
        private readonly HospitalContext _context;

        public PacienteService(HospitalContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PacienteListaVM>> ListaPacientes()
        {
            return await _context.VwPacientes.ToListAsync();
        }

        public async Task<bool> RegistrarPaciente(PacienteCrearVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var nuevoPaciente = new Paciente
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dpi = model.Dpi,
                    FechaNacimiento = model.FechaNacimiento,
                    Telefono = model.Telefono,
                    Correo = model.Correo,
                    IdGenero = model.IdGenero,
                    Direccion = model.Direccion
                };

                _context.Add(nuevoPaciente);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<PacienteEditarVM> ObtenerPacienteParaEditar(int idPaciente)
        {
            // Buscamos el médico incluyendo su relación con el Usuario del sistema
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdPaciente== idPaciente);

            if (paciente == null) return null;

            return new PacienteEditarVM
            {
                IdPaciente = paciente.IdPaciente,
                Nombre = paciente.Nombre,
                Apellido = paciente.Apellido,
                Dpi = paciente.Dpi,
                FechaNacimiento = paciente.FechaNacimiento,
                Telefono = paciente.Telefono,
                Correo = paciente.Correo,
                IdGenero = paciente.IdGenero,
                Direccion = paciente.Direccion,
            };
        }

        public async Task<bool> ActualizarPaciente(PacienteEditarVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Obtener el registro del paciente de la BD
                var paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.IdPaciente == model.IdPaciente);

                if(paciente == null) return false;

                paciente.Nombre = model.Nombre;
                paciente.Apellido = model.Apellido;
                paciente.Dpi = model.Dpi;
                paciente.FechaNacimiento = model.FechaNacimiento;
                paciente.Telefono = model.Telefono;
                paciente.Correo = model.Correo;
                paciente.IdGenero = model.IdGenero;
                paciente.Direccion = model.Direccion;

                _context.Pacientes.Update(paciente);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }


    }
}
