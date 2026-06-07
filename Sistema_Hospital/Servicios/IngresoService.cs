using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sistema_Hospital.Servicios
{
    public interface IIngresoService
    {
        Task<IEnumerable<IngresoListaVM>> ListaIngresos();
        Task<bool> RegistrarIngreso(IngresoCrearVM model, int idUsuario);
        Task<IngresoCrearVM> PrepararListasDesplegables();
        Task<bool> DarDeAlta(int idIngreso, int idUsuario);
    }

    public class IngresoService : IIngresoService
    {
        private readonly HospitalContext _context;
        private readonly IBitacoraService _bitacoraService; // <-- Inyectamos la bitácora

        public IngresoService(HospitalContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<IEnumerable<IngresoListaVM>> ListaIngresos()
        {
            return await _context.Ingresos
                .Include(i => i.IdPacienteNavigation)
                .Include(i => i.IdAreaNavigation)
                .Include(i => i.IdEstadoNavigation)
                .Select(i => new IngresoListaVM
                {
                    IdIngreso = i.IdIngreso,
                    Motivo = i.Motivo,
                    PacienteCompleto = i.IdPacienteNavigation.Nombre + " " + i.IdPacienteNavigation.Apellido,
                    AreaHospitalaria = i.IdAreaNavigation.NombreArea,
                    Estado = i.IdEstadoNavigation.NombreEstado,
                    FechaIngreso = i.FechaIngreso,
                    FechaSalida = i.FechaSalida
                })
                .ToListAsync();
        }

        public async Task<bool> RegistrarIngreso(IngresoCrearVM model, int idUsuario)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevoIngreso = new Ingreso
                {
                    Motivo = model.Motivo,
                    IdPaciente = model.IdPaciente,
                    IdEstado = model.IdEstado,
                    IdArea = model.IdArea,
                    FechaIngreso = DateTime.Now
                };

                _context.Ingresos.Add(nuevoIngreso);
                await _context.SaveChangesAsync();

                // Registrar en la bitácora
                string descripcionAccion = $"Registró un nuevo ingreso hospitalario para el paciente ID: {model.IdPaciente} en el área ID: {model.IdArea}.";
                await _bitacoraService.RegistrarAccion(idUsuario, descripcionAccion);

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<IngresoCrearVM> PrepararListasDesplegables()
        {
            var model = new IngresoCrearVM();

            model.Pacientes = await _context.Pacientes
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = p.IdPaciente.ToString(),
                    Text = $"{p.Nombre} {p.Apellido} (DPI: {p.Dpi})"
                }).ToListAsync();

            model.Areas = await _context.AreaHospital
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.IdArea.ToString(),
                    Text = a.NombreArea
                }).ToListAsync();

            model.Estados = await _context.Estados
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.IdEstado.ToString(),
                    Text = e.NombreEstado
                }).ToListAsync();

            return model;
        }

        public async Task<bool> DarDeAlta(int idIngreso, int idUsuario)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ingreso = await _context.Ingresos
                    .FirstOrDefaultAsync(i => i.IdIngreso == idIngreso);

                if (ingreso == null) return false;

                // Asignamos la fecha y hora actual como salida
                ingreso.FechaSalida = DateTime.Now;

                _context.Ingresos.Update(ingreso);
                await _context.SaveChangesAsync();

                // Registrar en la bitácora
                string descripcionAccion = $"Dio de alta al paciente del ingreso clínico con ID: {idIngreso}.";
                await _bitacoraService.RegistrarAccion(idUsuario, descripcionAccion);

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