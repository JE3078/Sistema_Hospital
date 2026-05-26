using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Sistema_Hospital.Servicios;
using System.Security.Claims;

namespace Sistema_Hospital.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService authService;
        private readonly IBitacoraService bitacoraService;

        public AuthController(IAuthService authService, IBitacoraService bitacoraService)
        {
            this.authService = authService;
            this.bitacoraService = bitacoraService;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var usuario = await authService.ValidarUsuario(username, password);

            if (usuario != null)
            {

                //Registro en bitacora: login exitoso
                await bitacoraService.RegistrarAccion(usuario.IdUsuario, "Inicio de Sesión exitoso");

                var claims = new List<Claim>
                {
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim("ID_Usuario", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Usuario o Contraseña incorrectos";

                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {

            // Recuperamos el ID del usuario actual desde las Claims antes de cerrar la sesión
            var idClaim = User.FindFirst("ID_Usuario")?.Value;
            if (int.TryParse(idClaim, out int idUsuario))
            {
                // 2. REGISTRO EN BITÁCORA (Cierre de sesión)
                await bitacoraService.RegistrarAccion(idUsuario, "Cierre de sesión manual.");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
