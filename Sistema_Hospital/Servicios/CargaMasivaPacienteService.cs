using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;

namespace Sistema_Hospital.Servicios
{
    // ─── Resultado ───────────────────────────────────────────────────────────
    public class ResultadoCargaMasivaPaciente
    {
        public int RegistrosCargados { get; set; }
        public int RegistrosOmitidos { get; set; }
        public List<string> Errores { get; set; } = new();
        public bool Exitoso => !Errores.Any() || RegistrosCargados > 0;
    }

    // ─── Interfaz ────────────────────────────────────────────────────────────
    public interface ICargaMasivaPacienteService
    {
        Task<ResultadoCargaMasivaPaciente> CargarPacientesDesdeCSV(Stream archivoCsv);
    }

    // ─── Implementación ──────────────────────────────────────────────────────
    public class CargaMasivaPacienteService : ICargaMasivaPacienteService
    {
        private readonly HospitalContext _context;
        private readonly int[] _generosValidos = { 1, 2, 3 };

        public CargaMasivaPacienteService(HospitalContext context)
        {
            _context = context;
        }

        public async Task<ResultadoCargaMasivaPaciente> CargarPacientesDesdeCSV(Stream archivoCsv)
        {
            var resultado = new ResultadoCargaMasivaPaciente();
            var pacientesNuevos = new List<Paciente>();

            // DPIs y correos ya existentes en BD para evitar duplicados
            var dpisExistentes = await _context.Pacientes.Select(p => p.Dpi).ToListAsync();
            var correosExistentes = await _context.Pacientes.Select(p => p.Correo.ToLower()).ToListAsync();

            using var reader = new StreamReader(archivoCsv, System.Text.Encoding.UTF8);

            // Saltar encabezado
            var encabezado = await reader.ReadLineAsync();
            if (encabezado == null)
            {
                resultado.Errores.Add("El archivo está vacío.");
                return resultado;
            }

            int numeroFila = 2;
            string? linea;
            while ((linea = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(linea)) { numeroFila++; continue; }

                try
                {
                    var cols = linea.Split(',');

                    if (cols.Length < 8)
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: Faltan columnas (se esperan 8).");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    var nombre = cols[0].Trim();
                    var apellido = cols[1].Trim();
                    var dpi = cols[2].Trim();
                    var fechaStr = cols[3].Trim();
                    var telefono = cols[4].Trim();
                    var correo = cols[5].Trim();
                    var generoStr = cols[6].Trim();
                    var direccion = cols[7].Trim();

                    // Nombre / Apellido
                    if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: Nombre o apellido vacío.");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // DPI — 13 dígitos y único
                    if (dpi.Length != 13 || !dpi.All(char.IsDigit))
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: DPI inválido '{dpi}' (debe tener 13 dígitos).");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }
                    if (dpisExistentes.Contains(dpi))
                    {
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // Fecha de nacimiento
                    if (!DateOnly.TryParse(fechaStr, out DateOnly fechaNacimiento))
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: Fecha inválida '{fechaStr}' (formato esperado: yyyy-MM-dd).");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // Teléfono — 8 dígitos
                    if (telefono.Length != 8 || !telefono.All(char.IsDigit))
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: Teléfono inválido '{telefono}' (debe tener 8 dígitos).");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // Correo único
                    if (correosExistentes.Contains(correo.ToLower()))
                    {
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // ID_Genero
                    if (!int.TryParse(generoStr, out int idGenero) || !_generosValidos.Contains(idGenero))
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: ID_Genero inválido '{generoStr}' (valores válidos: 1, 2, 3).");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    pacientesNuevos.Add(new Paciente
                    {
                        Nombre = nombre,
                        Apellido = apellido,
                        Dpi = dpi,
                        FechaNacimiento = fechaNacimiento,
                        Telefono = telefono,
                        Correo = correo,
                        IdGenero = idGenero,
                        Direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion
                    });

                    // Registrar en memoria para evitar duplicados dentro del mismo archivo
                    dpisExistentes.Add(dpi);
                    correosExistentes.Add(correo.ToLower());
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Fila {numeroFila}: Error inesperado — {ex.Message}");
                    resultado.RegistrosOmitidos++;
                }

                numeroFila++;
            }

            // ─── Transacción ACID ─────────────────────────────────────────────
            if (pacientesNuevos.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Pacientes.AddRangeAsync(pacientesNuevos);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    resultado.RegistrosCargados = pacientesNuevos.Count;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    resultado.Errores.Add($"Error al guardar en base de datos: {ex.Message}");
                    resultado.RegistrosCargados = 0;
                }
            }

            return resultado;
        }
    }
}