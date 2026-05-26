using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;

namespace Sistema_Hospital.Servicios
{
    public interface IAuthService
    {
        Task<UsuarioSistema?> ValidarUsuario(string username, string password);
    }
    public class AuthService : IAuthService
    {
        private readonly HospitalContext _context;
        public AuthService(HospitalContext context)
        {
            _context = context;
        }

        public async Task<UsuarioSistema?> ValidarUsuario(string username, string password)
        {
            var usuario = await _context.UsuarioSistemas.
                FirstOrDefaultAsync(u => u.Username == username && u.Estado == true);

            if (usuario is null)
            {
                return null;
            }

            bool passwordValido = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);



            if(passwordValido)
            {
                return usuario;
            }

            return null;
        }
    }
}
