using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;
using Sistema_Hospital.Servicios;

namespace Sistema_Hospital.Controllers
{
    // Al llamarlo "UsuarioController", ASP.NET automáticamente asociará la URL como "/Usuario"
    [Authorize(Roles = "1")] // Puedes descomentarlo cuando ya verifiques que sirve
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IBitacoraService bitacoraService;

        public UsuarioController(IUsuarioService usuarioService, IBitacoraService bitacoraService)
        {
            _usuarioService = usuarioService;
            this.bitacoraService = bitacoraService;
        }

        // Función auxiliar para obtener el ID del usuario en sesión
        private int ObtenerIdUsuarioEnSesion()
        {
            var idClaim = User.FindFirst("ID_Usuario")?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.ListarUsuarios();
            return View(usuarios);
        }


        #region Crear Usuario
        public IActionResult Crear()
        {
            var model = new UsuarioCrearVM
            {
                Roles = ObtenerListaRoles()
            };
            return View(model);
        }

        // POST: /Usuario/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioCrearVM model)
        {
            if (ModelState.IsValid)
            {
                var nuevoUsuario = new UsuarioSistema
                {
                    Username = model.Username,
                    IdRol = model.IdRol, // Aquí típicamente se creará Rol 1 (Admin) u otros
                    Estado = true
                };

                var exito = await _usuarioService.CrearUsuario(nuevoUsuario, model.Password);
                if (exito)
                {
                    int idUsuarioEnSesion = ObtenerIdUsuarioEnSesion();
                    await bitacoraService.RegistrarAccion(idUsuarioEnSesion, $"Creó un nuevo usuario administrativo en el sistema: '{model.Username}' con Rol ID: {model.IdRol}.");
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Error = "No se pudo registrar el usuario. El Username ya está en uso.";
            }

            model.Roles = ObtenerListaRoles();
            return View(model);
        }

        #endregion

        #region Editar Usuario
        // GET: /Usuario/Edit/5
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _usuarioService.ListarUsuarioPorID(id);
            if (usuario == null) return NotFound();

            var model = new UsuarioEditarVM
            {
                IdUsuario = usuario.IdUsuario,
                Username = usuario.Username,
                IdRol = usuario.IdRol,
                Estado = usuario.Estado ?? true,
                Roles = ObtenerListaRoles()
            };

            return View(model);
        }

        // POST: /Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, UsuarioEditarVM model)
        {
            if (id != model.IdUsuario) return NotFound();

            if (ModelState.IsValid)
            {
                // 1. OBTENER EL ESTADO ACTUAL DE LA BD ANTES DE GUARDAR LOS CAMBIOS
                // Esto nos permite saber qué tenía asignado el usuario originalmente
                var usuarioOriginal = await _usuarioService.ListarUsuarioPorID(id);

                if (usuarioOriginal == null) return NotFound();

                int rolAnterior = usuarioOriginal.IdRol; // Guardamos el ID del rol viejo

                // 2. PREPARAR EL OBJETO CON LOS NUEVOS CAMBIOS
                var usuarioActualizado = new UsuarioSistema
                {
                    IdUsuario = model.IdUsuario,
                    Username = model.Username,
                    IdRol = model.IdRol,
                    Estado = model.Estado
                };

                var exito = await _usuarioService.ActualizarUsuario(usuarioActualizado, model.NuevoPassword);
                if (exito)
                {
                    // 3. EVALUAR QUÉ FUE LO QUE CAMBIÓ PARA LA BITÁCORA
                    string cambioPassword = string.IsNullOrEmpty(model.NuevoPassword) ? "No" : "Sí";
                    string estadoTexto = model.Estado ? "Activo" : "Inactivo";

                    // Evaluamos si el rol enviado es diferente al que tenía originalmente
                    string detalleRol = (rolAnterior != model.IdRol)
                        ? $"Sí (De Rol ID {rolAnterior} a Rol ID {model.IdRol})"
                        : "No";

                    // 4. REGISTRO DETALLADO EN BITÁCORA
                    int adminId = ObtenerIdUsuarioEnSesion();
                    await bitacoraService.RegistrarAccion(adminId,
                        $"Modificó al usuario ID {model.IdUsuario} ('{model.Username}'). " +
                        $"Cambió Rol: {detalleRol}, Cambió Contraseña: {cambioPassword}, Estado: {estadoTexto}.");

                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Error = "Error al intentar actualizar los cambios en la base de datos.";
            }

            model.Roles = ObtenerListaRoles();
            return View(model);
        }

        #endregion


        private List<SelectListItem> ObtenerListaRoles()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Administrador" },
                //new SelectListItem { Value = "2", Text = "Médico" },
                //new SelectListItem { Value = "3", Text = "Enfermero" }
            };
        }
    }
}