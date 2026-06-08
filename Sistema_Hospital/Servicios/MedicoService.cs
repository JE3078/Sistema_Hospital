using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;
using System.Security.Cryptography; // <-- Requerido para AES
using System.Text;

namespace Sistema_Hospital.Servicios
{
    public interface IMedicoService
    {
        Task<bool> ActualizarMedico(MedicoEditarVM model);
        Task<bool> EliminarMedico(int idMedico);
        Task<IEnumerable<MedicoListaVM>> ListaMedicos();
        Task<MedicoEditarVM?> ObtenerMedicoParaEditar(int idMedico);
        Task<bool> RegistrarMedico(MedicoCrearVM model);
    }

    public class MedicoService : IMedicoService
    {
        private readonly HospitalContext _context;

        // Configuración de cifrado local en la clase
        // IMPORTANTE: Cambia esta clave por una de 32 caracteres y ponla idealmente en appsettings.json en producción
        private readonly byte[] _claveAes = Encoding.UTF8.GetBytes("TuClaveSuperSecretaDe32Bytes123!".PadRight(32).Substring(0, 32));
        private readonly byte[] _ivAes = new byte[16]; // Vector de inicialización fijo para consistencia

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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Paso 1: Insertar la Cuenta de Usuario
                var nuevoUsuario = new UsuarioSistema
                {
                    Username = model.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    IdRol = 2,
                    Estado = true
                };

                _context.UsuarioSistemas.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                // Paso 2: Insertar los Datos Clínicos (CIFRANDO EL DPI LOCALMENTE)
                var nuevoMedico = new Medico
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dpi = CifrarTexto(model.DPI), // <-- Llama al método privado de la clase
                    Colegiado = model.Colegiado,
                    FechaNacimiento = DateOnly.FromDateTime(model.FechaNacimiento),
                    Telefono = model.Telefono,
                    Correo = model.Correo,
                    IdGenero = model.IdGenero,
                    IdEspecialidad = model.IdEspecialidad,
                    Direccion = model.Direccion,
                    IdUsuario = nuevoUsuario.IdUsuario
                };

                _context.Medicos.Add(nuevoMedico);
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

        public async Task<MedicoEditarVM?> ObtenerMedicoParaEditar(int idMedico)
        {
            var medico = await _context.Medicos
                .Include(m => m.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdMedico == idMedico);

            if (medico == null) return null;

            return new MedicoEditarVM
            {
                IdMedico = medico.IdMedico,
                Nombre = medico.Nombre,
                Apellido = medico.Apellido,
                DPI = DescifrarTexto(medico.Dpi), // <-- Llama al método privado para descifrar el DPI
                Colegiado = medico.Colegiado,
                FechaNacimiento = medico.FechaNacimiento.ToDateTime(TimeOnly.MinValue),
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
                var medico = await _context.Medicos
                    .Include(m => m.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(m => m.IdMedico == model.IdMedico);

                if (medico == null) return false;

                // Actualizar datos de la Ficha Médica
                medico.Nombre = model.Nombre;
                medico.Apellido = model.Apellido;
                medico.Dpi = CifrarTexto(model.DPI); // <-- Cifra el DPI editado
                medico.Colegiado = model.Colegiado;
                medico.FechaNacimiento = DateOnly.FromDateTime(model.FechaNacimiento);
                medico.Telefono = model.Telefono;
                medico.Correo = model.Correo;
                medico.IdGenero = model.IdGenero;
                medico.IdEspecialidad = model.IdEspecialidad;
                medico.Direccion = model.Direccion;

                // Actualizar contraseña del Usuario
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

        public async Task<bool> EliminarMedico(int idMedico)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Traemos al médico incluyendo su usuario asignado
                var medico = await _context.Medicos
                    .Include(m => m.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(m => m.IdMedico == idMedico);

                if (medico == null) return false;

                // Paso 1: Borrado lógico del Médico
                medico.Estado = false;
                _context.Medicos.Update(medico);

                // Paso 2: Deshabilitar también su cuenta de usuario (si tiene una asignada)
                if (medico.IdUsuarioNavigation != null)
                {
                    medico.IdUsuarioNavigation.Estado = false;
                    _context.UsuarioSistemas.Update(medico.IdUsuarioNavigation);
                }

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

        // ==========================================
        // MÉTODOS PRIVADOS DE CIFRADO (AES-256)
        // ==========================================

        private string CifrarTexto(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano)) return textoPlano;

            using Aes aes = Aes.Create();
            aes.Key = _claveAes;
            aes.IV = _ivAes;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(textoPlano);
                }
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        private string DescifrarTexto(string textoCifrado)
        {
            if (string.IsNullOrEmpty(textoCifrado)) return textoCifrado;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = _claveAes;
                aes.IV = _ivAes;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream ms = new MemoryStream(Convert.FromBase64String(textoCifrado));
                using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
            catch
            {
                // Retorna el valor original si falla (útil si hay registros viejos sin cifrar)
                return textoCifrado;
            }
        }
    }
}