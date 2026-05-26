using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;

namespace Sistema_Hospital.Servicios
{
    public interface IMedicoService
    {
        Task<bool> ActualizarMedico(MedicoEditarVM model);
        Task<IEnumerable<MedicoListaVM>> ListaMedicos();
        Task<MedicoEditarVM?> ObtenerMedicoParaEditar(int idMedico);
        Task<bool> RegistrarMedico(MedicoCrearVM model);
    }

    public class MedicoService : IMedicoService
    {
        private readonly HospitalContext _context;

        public MedicoService(HospitalContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicoListaVM>> ListaMedicos()
        {
            return await _context.VwMedicosActivos.ToListAsync();
        }
        public async Task<bool> RegistrarMedico(MedicoCrearVM model)
        {
            // CUMPLIMIENTO MÓDULO 5: Transacción ACID con EF Core
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Paso 1: Insertar la Cuenta de Usuario en [Administracion].[Usuario_Sistema]
                var nuevoUsuario = new UsuarioSistema
                {
                    Username = model.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), // Hashing seguro BCrypt
                    IdRol = 2, // Rol ID fijo para Médico
                    Estado = true
                };

                _context.UsuarioSistemas.Add(nuevoUsuario);
                await _context.SaveChangesAsync(); // SQL Server genera e inyecta el ID_Usuario aquí

                // Paso 2: Insertar los Datos Clínicos en [Hospitalario].[Medico]
                var nuevoMedico = new Medico
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dpi = model.DPI,
                    Colegiado = model.Colegiado,
                    FechaNacimiento = DateOnly.FromDateTime(model.FechaNacimiento), // Mapeado a [date] de SQL
                    Telefono = model.Telefono,
                    Correo = model.Correo,
                    IdGenero = model.IdGenero,
                    IdEspecialidad = model.IdEspecialidad,
                    Direccion = model.Direccion,
                    IdUsuario = nuevoUsuario.IdUsuario // Vinculamos la FK con el ID recién generado
                };

                _context.Medicos.Add(nuevoMedico);
                await _context.SaveChangesAsync();

                // Si ambos pasos fueron exitosos, consolidamos la operación en SQL Server
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                // Si algo falla (ej. violación de restricción UNIQUE en DPI o Username), se deshace todo
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<MedicoEditarVM?> ObtenerMedicoParaEditar(int idMedico)
        {
            // Buscamos el médico incluyendo su relación con el Usuario del sistema
            var medico = await _context.Medicos
                .Include(m => m.IdUsuarioNavigation) // Ajusta el nombre de la propiedad de navegación si varía en tu DbContext
                .FirstOrDefaultAsync(m => m.IdMedico == idMedico);

            if (medico == null) return null;

            // Convertimos la entidad física al ViewModel de edición
            return new MedicoEditarVM
            {
                IdMedico = medico.IdMedico,
                Nombre = medico.Nombre,
                Apellido = medico.Apellido,
                DPI = medico.Dpi,
                Colegiado = medico.Colegiado,
                FechaNacimiento = medico.FechaNacimiento.ToDateTime(TimeOnly.MinValue), // Conversión de DateOnly a DateTime
                Telefono = medico.Telefono,
                Correo = medico.Correo,
                IdGenero = medico.IdGenero,
                IdEspecialidad = medico.IdEspecialidad,
                Direccion = medico.Direccion,
                Username = medico.IdUsuarioNavigation?.Username ?? "Sin Usuario"
            };
        }

        public async Task<bool> ActualizarMedico(MedicoEditarVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Obtener el registro del médico de la BD
                var medico = await _context.Medicos
                    .Include(m => m.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(m => m.IdMedico == model.IdMedico);

                if (medico == null) return false;

                // 2. Actualizar datos de la Ficha Médica
                medico.Nombre = model.Nombre;
                medico.Apellido = model.Apellido;
                medico.Dpi = model.DPI;
                medico.Colegiado = model.Colegiado;
                medico.FechaNacimiento = DateOnly.FromDateTime(model.FechaNacimiento);
                medico.Telefono = model.Telefono;
                medico.Correo = model.Correo;
                medico.IdGenero = model.IdGenero;
                medico.IdEspecialidad = model.IdEspecialidad;
                medico.Direccion = model.Direccion;

                // 3. Actualizar contraseña del Usuario (Solo si el administrador escribió una nueva)
                if (!string.IsNullOrEmpty(model.NuevaPassword) && medico.IdUsuarioNavigation != null)
                {
                    medico.IdUsuarioNavigation.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NuevaPassword);
                    _context.UsuarioSistemas.Update(medico.IdUsuarioNavigation);
                }

                _context.Medicos.Update(medico);
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