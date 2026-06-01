using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;

namespace Sistema_Hospital.Servicios
{
    public interface IMedicamentoService
    {
        Task<bool> ActualizarMedicamento(Medicamento model);
        Task<IEnumerable<Medicamento>> ListaMedicamentos();
        Task<Medicamento?> ObtenerMedicamentoParaEditar(int idMedicamento);
        Task<bool> RegistrarMedicamento(Medicamento model);
    }
    public class MedicamentoService : IMedicamentoService
    {
        private readonly HospitalContext context;

        public MedicamentoService(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Medicamento>> ListaMedicamentos()
        {
            return await context.Medicamentos.ToListAsync();
        }
        public async Task<bool> RegistrarMedicamento(Medicamento model)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                context.Add(model);
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

        public async Task<Medicamento?> ObtenerMedicamentoParaEditar(int idMedicamento)
        {
            // Buscamos el médico incluyendo su relación con el Usuario del sistema
            var medicamento = await context.Medicamentos
                .FirstOrDefaultAsync(m => m.IdMedicamento == idMedicamento);

            if (medicamento == null) return null;

            return new Medicamento
            {
                IdMedicamento = medicamento.IdMedicamento,
                Nombre = medicamento.Nombre,
                Descripcion = medicamento.Descripcion,
                Stock = medicamento.Stock,
                Precio = medicamento.Precio,

            };
        }

        public async Task<bool> ActualizarMedicamento(Medicamento model)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Obtener el registro del médico de la BD
                var medicamento = await context.Medicamentos
                    .FirstOrDefaultAsync(m => m.IdMedicamento == model.IdMedicamento);

                if (medicamento == null) return false;

                // 2. Actualizar datos de la Ficha Médica
                medicamento.Nombre = model.Nombre;
                medicamento.Descripcion = model.Descripcion;
                medicamento.Stock = model.Stock;
                medicamento.Precio = model.Precio;

                context.Medicamentos.Update(medicamento);
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
    }
}
