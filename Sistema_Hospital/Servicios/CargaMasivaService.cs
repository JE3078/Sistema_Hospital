using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;

namespace Sistema_Hospital.Servicios
{
    // ─── Resultado de la carga ───────────────────────────────────────────────
    public class ResultadoCargaMasiva
    {
        public int RegistrosCargados { get; set; }
        public int RegistrosOmitidos { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<MedicamentoCargado> Cargados { get; set; } = new();
        public List<MedicamentoOmitido> Omitidos { get; set; } = new();
        public bool Exitoso => !Errores.Any() || RegistrosCargados > 0;
    }

    public class MedicamentoCargado
    {
        public string Nombre { get; set; } = "";
        public int Stock { get; set; }
        public decimal Precio { get; set; }
    }

    public class MedicamentoOmitido
    {
        public string Nombre { get; set; } = "";
        public string Motivo { get; set; } = "";
    }

    // ─── Interfaz ────────────────────────────────────────────────────────────
    public interface ICargaMasivaService
    {
        Task<ResultadoCargaMasiva> CargarMedicamentosDesdeExcel(Stream archivoExcel);
    }

    // ─── Implementación ──────────────────────────────────────────────────────
    public class CargaMasivaService : ICargaMasivaService
    {
        private readonly HospitalContext _context;

        public CargaMasivaService(HospitalContext context)
        {
            _context = context;
        }

        public async Task<ResultadoCargaMasiva> CargarMedicamentosDesdeExcel(Stream archivoExcel)
        {
            var resultado = new ResultadoCargaMasiva();
            var medicamentosNuevos = new List<Medicamento>();

            // Nombres ya existentes en BD
            var nombresExistentes = await _context.Medicamentos
                .Select(m => m.Nombre.ToLower())
                .ToListAsync();

            using var workbook = new XLWorkbook(archivoExcel);
            var hoja = workbook.Worksheet(1);
            var filas = hoja.RangeUsed().RowsUsed().Skip(1);

            int numeroFila = 2;
            foreach (var fila in filas)
            {
                try
                {
                    var nombre = fila.Cell(1).GetString().Trim();
                    var descripcion = fila.Cell(2).GetString().Trim();
                    var stockStr = fila.Cell(3).GetString().Trim();
                    var precioStr = fila.Cell(4).GetString().Trim();

                    // Nombre vacío
                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: El nombre no puede estar vacío.");
                        resultado.Omitidos.Add(new MedicamentoOmitido
                        {
                            Nombre = $"(fila {numeroFila} sin nombre)",
                            Motivo = "Nombre vacío"
                        });
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // Duplicado en BD
                    if (nombresExistentes.Contains(nombre.ToLower()))
                    {
                        resultado.Omitidos.Add(new MedicamentoOmitido
                        {
                            Nombre = nombre,
                            Motivo = "Ya existe en la base de datos"
                        });
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // Stock inválido
                    if (!int.TryParse(stockStr, out int stock) || stock < 0)
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: Stock inválido '{stockStr}'.");
                        resultado.Omitidos.Add(new MedicamentoOmitido
                        {
                            Nombre = nombre,
                            Motivo = $"Stock inválido: '{stockStr}'"
                        });
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // Precio inválido
                    var precioLimpio = precioStr.Replace("Q", "").Replace("$", "").Trim();
                    if (!decimal.TryParse(precioLimpio,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out decimal precio) || precio < 0)
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: Precio inválido '{precioStr}'.");
                        resultado.Omitidos.Add(new MedicamentoOmitido
                        {
                            Nombre = nombre,
                            Motivo = $"Precio inválido: '{precioStr}'"
                        });
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // Registro válido
                    medicamentosNuevos.Add(new Medicamento
                    {
                        Nombre = nombre,
                        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion,
                        Stock = stock,
                        Precio = precio
                    });

                    resultado.Cargados.Add(new MedicamentoCargado
                    {
                        Nombre = nombre,
                        Stock = stock,
                        Precio = precio
                    });

                    // Evitar duplicados dentro del mismo archivo
                    nombresExistentes.Add(nombre.ToLower());
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Fila {numeroFila}: Error inesperado — {ex.Message}");
                    resultado.RegistrosOmitidos++;
                }

                numeroFila++;
            }

            // ─── Transacción ACID ─────────────────────────────────────────────
            if (medicamentosNuevos.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Medicamentos.AddRangeAsync(medicamentosNuevos);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    resultado.RegistrosCargados = medicamentosNuevos.Count;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    resultado.Errores.Add($"Error al guardar en base de datos: {ex.Message}");
                    resultado.RegistrosCargados = 0;
                    resultado.Cargados.Clear();
                }
            }

            return resultado;
        }
    }
}