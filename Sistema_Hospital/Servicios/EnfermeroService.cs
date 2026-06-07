using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;
using System.Security.Cryptography; // <-- Requerido para AES
using System.Text;

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

        // Configuración de cifrado local en la clase (Debe coincidir con la clave usada en Médicos si comparten la lógica de lectura)
        private readonly byte[] _claveAes = Encoding.UTF8.GetBytes("TuClaveSuperSecretaDe32Bytes123!".PadRight(32).Substring(0, 32));
        private readonly byte[] _ivAes = new byte[16];

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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    IdRol = 3, // Rol ID fijo para enfermero
                    Estado = true
                };

                _context.UsuarioSistemas.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                // Paso 2: Insertar los Datos Clínicos en [Hospitalario].[Enfemero] (CIFRANDO EL DPI)
                var nuevoEnfermero = new Enfermero
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dpi = CifrarTexto(model.DPI), // <-- MODIFICADO: Se cifra el DPI
                    Telefono = model.Telefono,
                    Correo = model.Correo,
                    IdGenero = model.IdGenero,
                    IdUsuario = nuevoUsuario.IdUsuario
                };

                _context.Enfermeros.Add(nuevoEnfermero);
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
                DPI = DescifrarTexto(enfermero.Dpi), // <-- MODIFICADO: Se descifra el DPI para la vista
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

                // 2. Modificar datos de la Ficha del Enfermero (RE-CIFRANDO EL DPI)
                enfermero.Nombre = model.Nombre;
                enfermero.Apellido = model.Apellido;
                enfermero.Dpi = CifrarTexto(model.DPI); // <-- MODIFICADO: Se vuelve a cifrar el valor modificado
                enfermero.Telefono = model.Telefono;
                enfermero.Correo = model.Correo;
                enfermero.IdGenero = model.IdGenero;

                // 3. Modificar credenciales
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
                // Si la base de datos contiene algún dato viejo sin encriptar, 
                // previene el quiebre de la app devolviendo el texto plano.
                return textoCifrado;
            }
        }
    }
}