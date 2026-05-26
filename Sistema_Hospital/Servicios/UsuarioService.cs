using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Data;
using Sistema_Hospital.Models;

namespace Sistema_Hospital.Servicios
{
    public interface IUsuarioService
    {
        Task<bool> ActualizarUsuario(UsuarioSistema usuario, string? nuevoPasswordClaro = null);
        Task<bool> CambiarEstado(int id, bool estado);
        Task<bool> CrearUsuario(UsuarioSistema usuario, string passwordSinHash);
        Task<UsuarioSistema> ListarUsuarioPorID(int ID_Usuario);
        Task<IEnumerable<UsuarioSistema>> ListarUsuarios();
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly HospitalContext _context;
        private readonly IBitacoraService bitacoraService;

        public UsuarioService(HospitalContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            this.bitacoraService = bitacoraService;
        }

        public async Task<IEnumerable<UsuarioSistema>> ListarUsuarios()
        {
            return await _context.UsuarioSistemas.ToListAsync();
        }

        public async Task<UsuarioSistema> ListarUsuarioPorID(int ID_Usuario)
        {
            return await _context.UsuarioSistemas.FindAsync(ID_Usuario);
        }

        public async Task<bool> CrearUsuario(UsuarioSistema usuario, string passwordSinHash)
        {
            try
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordSinHash);
                usuario.Estado = true;

                _context.UsuarioSistemas.Add(usuario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> ActualizarUsuario(UsuarioSistema usuario, string? nuevoPasswordClaro = null)
        {
            try
            {
                var usuarioExistente = await _context.UsuarioSistemas.FindAsync(usuario.IdUsuario);
                if (usuarioExistente == null) return false;

                usuarioExistente.Username = usuario.Username;
                usuarioExistente.IdRol = usuario.IdRol;
                usuarioExistente.Estado = usuario.Estado;

                // Si el administrador digitó una nueva contraseña, la actualizamos hasheada
                if (!string.IsNullOrEmpty(nuevoPasswordClaro))
                {
                    usuarioExistente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevoPasswordClaro);
                }

                _context.UsuarioSistemas.Update(usuarioExistente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CambiarEstado(int id, bool estado)
        {
            var usuario = await _context.UsuarioSistemas.FindAsync(id);
            if (usuario == null) return false;

            usuario.Estado = estado;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
