using Hotel.Data;
using Hotel.Data.Entidades;
using Hotel.Models;
using Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Controllers
{
    [Authorize(Roles = Roles.Personal)]
    public class UsuarioController : Controller
    {
        private readonly HotelContext _contexto;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UsuarioHotelService _usuarioHotelService;

        public UsuarioController(
            HotelContext contexto,
            UserManager<IdentityUser> userManager,
            UsuarioHotelService usuarioHotelService)
        {
            _contexto = contexto;
            _userManager = userManager;
            _usuarioHotelService = usuarioHotelService;
        }

        // GET: Usuario
        public async Task<IActionResult> Index(
            string? buscar,
            string? rol)
        {
            var consulta = _contexto.Usuarios
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(u =>
                    u.Nombre.Contains(buscar) ||
                    u.Cedula.Contains(buscar) ||
                    u.Correo.Contains(buscar));
            }

            if (!string.IsNullOrWhiteSpace(rol))
            {
                consulta = consulta.Where(u => u.Rol == rol);
            }

            var usuarios = await consulta
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.Rol = rol;

            return View(usuarios);
        }

        // GET: Usuario/Crear
        [Authorize(Roles = Roles.Administrador + "," + Roles.Recepcionista)]
        public IActionResult Crear()
        {
            var modelo = new UsuarioViewModel();

            // El recepcionista solamente puede registrar clientes.
            if (User.IsInRole(Roles.Recepcionista))
            {
                modelo.Rol = Roles.Cliente;
            }

            return View(modelo);
        }

        // POST: Usuario/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador + "," + Roles.Recepcionista)]
        public async Task<IActionResult> Crear(
            UsuarioViewModel model)
        {
            // Valida los roles permitidos según el usuario que registra.
            if (User.IsInRole(Roles.Recepcionista))
            {
                model.Rol = Roles.Cliente;
            }
            else if (!Roles.Todos.Contains(model.Rol))
            {
                ModelState.AddModelError(
                    nameof(model.Rol),
                    "El rol seleccionado no es válido.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuarioIdentity = new IdentityUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim()
            };

            var resultado = await _userManager.CreateAsync(
                usuarioIdentity,
                model.Password);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            var resultadoRol = await _userManager.AddToRoleAsync(
                usuarioIdentity,
                model.Rol);

            if (!resultadoRol.Succeeded)
            {
                await _userManager.DeleteAsync(usuarioIdentity);

                foreach (var error in resultadoRol.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            try
            {
                _usuarioHotelService.AsegurarRegistroAdministrativo(
                    usuarioIdentity.Id,
                    model.Nombre,
                    model.Cedula,
                    model.Telefono,
                    model.Direccion,
                    model.Rol);
            }
            catch
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible guardar los datos del usuario.");

                return View(model);
            }

            TempData["Mensaje"] = "El usuario se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}