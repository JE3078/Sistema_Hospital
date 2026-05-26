using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;

namespace Sistema_Hospital.Servicios
{
    public interface IEnfermeroService
    {
        Task<bool> ActualizarEnfermero(EnfermeroEditarVM model);
        Task<IEnumerable<EnfermeroListaVM>> ListaEnfermeros();
        Task<EnfermeroEditarVM?> ObtenerEnfermeroParaEditar(int idEnfermero);
        Task<bool> RegistrarEnfermero(EnfermeroCrearVM model);
    }
    public class EnfermeroService : IEnfermeroService
    {
        private readonly HospitalContext _context;
        public EnfermeroService(HospitalContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EnfermeroListaVM>> ListaEnfermeros()
        {
            return await _context.VwEnfermerosActivos.ToListAsync();
        }

        public async Task<bool> RegistrarEnfermero(EnfermeroCrearVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Paso 1: Insertar la Cuenta de Usuario en [Administracion].[Usuario_Sistema]
                var nuevoUsuario = new UsuarioSistema
                {
                    Username = model.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), // Hashing seguro BCrypt
                    IdRol = 3, // Rol ID fijo para enfermero
                    Estado = true
                };

                _context.UsuarioSistemas.Add(nuevoUsuario);
                await _context.SaveChangesAsync(); // SQL Server genera e inyecta el ID_Usuario aquí

                // Paso 2: Insertar los Datos Clínicos en [Hospitalario].[Enfemero]

                var nuevoEnfermero = new Enfermero
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dpi = model.DPI,
                    Telefono = model.Telefono,
                    Correo = model.Correo,
                    IdGenero = model.IdGenero,
                    IdUsuario = nuevoUsuario.IdUsuario
                };

                _context.Enfermeros.Add(nuevoEnfermero);
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

        public async Task<EnfermeroEditarVM?> ObtenerEnfermeroParaEditar(int idEnfermero)
        {
            var enfermero = await _context.Enfermeros
                .Include(e => e.IdUsuarioNavigation)
                .FirstOrDefaultAsync(e => e.IdEnfermero == idEnfermero);

            if (enfermero == null) return null;

            return new EnfermeroEditarVM
            {
                IdEnfermero = enfermero.IdEnfermero,
                Nombre = enfermero.Nombre,
                Apellido = enfermero.Apellido,
                DPI = enfermero.Dpi,
                Telefono = enfermero.Telefono,
                Correo = enfermero.Correo,
                IdGenero = enfermero.IdGenero,
                Username = enfermero.IdUsuarioNavigation?.Username ?? "Sin Usuario"
            };
        }

        public async Task<bool> ActualizarEnfermero(EnfermeroEditarVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Obtener el registro actual
                var enfermero = await _context.Enfermeros
                    .Include(e => e.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(e => e.IdEnfermero == model.IdEnfermero);

                if (enfermero == null) return false;

                // 2. Modificar datos de la Ficha del Enfermero
                enfermero.Nombre = model.Nombre;
                enfermero.Apellido = model.Apellido;
                enfermero.Dpi = model.DPI;
                enfermero.Telefono = model.Telefono;
                enfermero.Correo = model.Correo;
                enfermero.IdGenero = model.IdGenero;

                // 3. Modificar credenciales (Solo si se ingresó un password nuevo)
                if (!string.IsNullOrEmpty(model.NuevaPassword) && enfermero.IdUsuarioNavigation != null)
                {
                    enfermero.IdUsuarioNavigation.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NuevaPassword);
                    _context.UsuarioSistemas.Update(enfermero.IdUsuarioNavigation);
                }

                _context.Enfermeros.Update(enfermero);
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
