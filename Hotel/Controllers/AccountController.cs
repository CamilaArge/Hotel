using Hotel.Models;
using Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UsuarioHotelService _usuarioHotelService;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            UsuarioHotelService usuarioHotelService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _usuarioHotelService = usuarioHotelService;
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                string.Empty,
                "Correo o contraseña incorrectos.");

            return View(model);
        }

        // GET: Account/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var resultado = await _userManager.CreateAsync(
                usuario, model.Password);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty, error.Description);
                }

                return View(model);
            }

            var resultadoRol = await _userManager.AddToRoleAsync(
                usuario, Roles.Cliente);

            if (!resultadoRol.Succeeded)
            {
                await _userManager.DeleteAsync(usuario);

                foreach (var error in resultadoRol.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty, error.Description);
                }

                return View(model);
            }

            try
            {
                // Crear o vincular el registro en la tabla Usuario
                _usuarioHotelService.AsegurarRegistroCliente(
                    usuario.Id,
                    model.Nombre,
                    model.Cedula,
                    model.Telefono,
                    model.Direccion);
            }
            catch
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible guardar sus datos personales. Contacte al administrador.");

                return View(model);
            }

            // Iniciar sesión después de guardar correctamente los datos
            await _signInManager.SignInAsync(
                usuario, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        // POST: Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}