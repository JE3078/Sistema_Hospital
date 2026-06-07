using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;
using System.Security.Cryptography;

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
                // --- LÓGICA DE CIFRADO SIMÉTRICO LOCAL ---
                string dpiCifrado = model.Dpi;
                if (!string.IsNullOrEmpty(model.Dpi))
                {
                    using Aes aes = Aes.Create();
                    // Clave de 32 bytes y IV de 16 bytes fijos para consistencia
                    aes.Key = System.Text.Encoding.UTF8.GetBytes("TuClaveSuperSecretaDe32Bytes123!".PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16];

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using MemoryStream ms = new MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(model.Dpi);
                        }
                    }
                    dpiCifrado = Convert.ToBase64String(ms.ToArray());
                }
                // ----------------------------------------

                var nuevoPaciente = new Paciente
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dpi = dpiCifrado, // <-- Guardamos el DPI cifrado
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
                .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);

            if (paciente == null) return null;

            // --- LÓGICA DE DESCIFRADO SIMÉTRICO LOCAL ---
            string dpiDescifrado = paciente.Dpi;
            if (!string.IsNullOrEmpty(paciente.Dpi))
            {
                try
                {
                    using Aes aes = Aes.Create();
                    aes.Key = System.Text.Encoding.UTF8.GetBytes("TuClaveSuperSecretaDe32Bytes123!".PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16];

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using MemoryStream ms = new MemoryStream(Convert.FromBase64String(paciente.Dpi));
                    using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                    using StreamReader sr = new StreamReader(cs);

                    dpiDescifrado = sr.ReadToEnd();
                }
                catch
                {
                    // Si falla (por si hay datos viejos en la BD sin cifrar), mantiene el valor original
                    dpiDescifrado = paciente.Dpi;
                }
            }
            // --------------------------------------------

            return new PacienteEditarVM
            {
                IdPaciente = paciente.IdPaciente,
                Nombre = paciente.Nombre,
                Apellido = paciente.Apellido,
                Dpi = dpiDescifrado, // <-- Enviamos el DPI descifrado a la vista
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

                if (paciente == null) return false;

                // --- LÓGICA DE CIFRADO SIMÉTRICO LOCAL ---
                string dpiCifrado = model.Dpi;
                if (!string.IsNullOrEmpty(model.Dpi))
                {
                    using Aes aes = Aes.Create();
                    aes.Key = System.Text.Encoding.UTF8.GetBytes("TuClaveSuperSecretaDe32Bytes123!".PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16];

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using MemoryStream ms = new MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(model.Dpi);
                        }
                    }
                    dpiCifrado = Convert.ToBase64String(ms.ToArray());
                }
                // ----------------------------------------

                paciente.Nombre = model.Nombre;
                paciente.Apellido = model.Apellido;
                paciente.Dpi = dpiCifrado; // <-- Guardamos el nuevo DPI cifrado
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
