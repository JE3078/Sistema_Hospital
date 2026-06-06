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
        public bool Exitoso => !Errores.Any() || RegistrosCargados > 0;
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

            // Nombres ya existentes en BD (para no duplicar)
            var nombresExistentes = await _context.Medicamentos
                .Select(m => m.Nombre.ToLower())
                .ToListAsync();

            using var workbook = new XLWorkbook(archivoExcel);
            var hoja = workbook.Worksheet(1);
            var filas = hoja.RangeUsed().RowsUsed().Skip(1); // saltar encabezado

            int numeroFila = 2;
            foreach (var fila in filas)
            {
                try
                {
                    var nombre = fila.Cell(1).GetString().Trim();
                    var descripcion = fila.Cell(2).GetString().Trim();
                    var stockStr = fila.Cell(3).GetString().Trim();
                    var precioStr = fila.Cell(4).GetString().Trim();

                    // Validaciones
                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: El nombre no puede estar vacío.");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    if (nombresExistentes.Contains(nombre.ToLower()))
                    {
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue; // ya existe, se omite silenciosamente
                    }

                    if (!int.TryParse(stockStr, out int stock) || stock < 0)
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: Stock inválido '{stockStr}'.");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    // Limpiar símbolo de moneda (Q, $, etc.) antes de parsear
                    var precioLimpio = precioStr.Replace("Q", "").Replace("$", "").Trim();
                    if (!decimal.TryParse(precioLimpio,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out decimal precio) || precio < 0)
                    {
                        resultado.Errores.Add($"Fila {numeroFila}: Precio inválido '{precioStr}'.");
                        resultado.RegistrosOmitidos++;
                        numeroFila++;
                        continue;
                    }

                    medicamentosNuevos.Add(new Medicamento
                    {
                        Nombre = nombre,
                        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion,
                        Stock = stock,
                        Precio = precio
                    });

                    // Registro en memoria para no duplicar en el mismo archivo
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
                }
            }

            return resultado;
        }
    }
}