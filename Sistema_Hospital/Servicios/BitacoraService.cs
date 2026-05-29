using Sistema_Hospital.Data;
using Sistema_Hospital.Models;

namespace Sistema_Hospital.Servicios
{
    public interface IBitacoraService
    {
        Task RegistrarAccion(int idUsuario, string accion);
    }
    public class BitacoraService : IBitacoraService
    {
        private readonly HospitalContext _context;

        public BitacoraService(HospitalContext context)
        {
            _context = context;   
        }

        public async Task RegistrarAccion(int idUsuario, string accion)
        {
            try
            {
                var log = new Bitacora
                {
                    IdUsuario = idUsuario,
                    Accion = accion,
                    FechaHora = DateTime.Now
                };

                _context.Bitacora.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
            }
        }
    }
}
