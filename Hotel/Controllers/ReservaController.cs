using System.Security.Claims;
using Hotel.Models;
using Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    [Authorize]
    public class ReservaController : Controller
    {
        private readonly ReservaService _reservaService;
        private readonly HabitacionService _habitacionService;
        private readonly FechaService _fechaService;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly UsuarioHotelService _usuarioHotelService;

        public ReservaController(ReservaService reservaService, HabitacionService habitacionService,
            FechaService fechaService, UserManager<IdentityUser> userManager, UsuarioHotelService usuarioHotelService)
        {
            _reservaService = reservaService;
            _habitacionService = habitacionService;
            _fechaService = fechaService;
            _userManager = userManager;
            _usuarioHotelService = usuarioHotelService;
        }

        private string IdUsuarioActual => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private bool EsPersonal => User.IsInRole(Roles.Administrador) || User.IsInRole(Roles.Recepcionista);

  
        [Authorize(Roles = Roles.Personal)]
        public IActionResult Index(FiltroReservas filtro)
        {
            ViewBag.Filtro = filtro;
            return View(_reservaService.Buscar(filtro));
        }

    
        [Authorize(Roles = Roles.Personal)]
        public IActionResult ObtenerReservas(int idHabitacion)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(idHabitacion);

            if (habitacion == null)
            {
                return NotFound();
            }

            ViewBag.Habitacion = habitacion;
            return View(_reservaService.ObtenerPorHabitacion(idHabitacion));
        }

      
        [Authorize(Roles = Roles.Cliente)]
        public IActionResult MisReservas()
        {
            return View(_reservaService.ObtenerPorUsuario(IdUsuarioActual));
        }

      
        [Authorize(Roles = Roles.Cliente)]
        public IActionResult ObtenerHabitaciones(DateTime? entrada, DateTime? salida, int? personas)
        {
            ViewBag.Entrada = entrada;
            ViewBag.Salida = salida;
            ViewBag.Personas = personas;

            if (entrada.HasValue && salida.HasValue)
            {
                if (salida.Value.Date <= entrada.Value.Date)
                {
                    ViewBag.Error = "La fecha de salida debe ser posterior a la fecha de entrada.";
                    return View(new List<Habitacion>());
                }

                if (entrada.Value.Date < _fechaService.ObtenerFechaActual().Date)
                {
                    ViewBag.Error = "La fecha de entrada no puede ser anterior a hoy.";
                    return View(new List<Habitacion>());
                }

                return View(_habitacionService.ObtenerDisponibles(entrada.Value, salida.Value, Math.Max(1, personas ?? 1)));
            }

            return View(_habitacionService.ObtenerActivas());
        }



        // GET: Reserva/Detalles/5
        public IActionResult Detalles(int id)
        {
            Reserva? reserva = ObtenerReservaVisible(id);

            if (reserva == null)
            {
                TempData["Mensaje"] = "No se encontró la reservación indicada.";
                return EsPersonal ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(ObtenerHabitaciones));
            }

            ViewBag.PuedeCancelar = PuedeCancelar(reserva, out _);
            return View(reserva);
        }

    
        public IActionResult DetallesParcial(int idReservacion)
        {
            Reserva? reserva = ObtenerReservaVisible(idReservacion);
            return reserva == null ? Content(string.Empty) : PartialView("_DetallesParcial", reserva);
        }


        public async Task<IActionResult> AgregarReserva(int? idHabitacion, DateTime? entrada, DateTime? salida, int? personas)
        {
            DateTime hoy = _fechaService.ObtenerFechaActual().Date;

            Reserva reserva = new()
            {
                IdHabitacion = idHabitacion ?? 0,
                FechaEntrada = entrada?.Date ?? hoy,
                FechaSalida = salida?.Date ?? hoy.AddDays(1),
                CantidadPersonas = Math.Max(1, personas ?? 1)
            };

          
            if (!EsPersonal && (idHabitacion == null || _habitacionService.ObtenerPorId(idHabitacion.Value) == null))
            {
                return RedirectToAction(nameof(ObtenerHabitaciones));
            }

            await CargarDatosFormulario(reserva);
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarReserva(Reserva reserva)
        {

            string idUsuario;

            if (EsPersonal)
            {
                idUsuario = reserva.IdentityIdUsuario ?? string.Empty;

                if (string.IsNullOrWhiteSpace(idUsuario) ||
                    await _userManager.FindByIdAsync(idUsuario) == null)
                {
                    ModelState.AddModelError(
                        nameof(Reserva.IdentityIdUsuario),
                        "Debe seleccionar el huésped.");
                }
            }
            else
            {
                idUsuario = IdUsuarioActual;
            }

            if (ModelState.IsValid)
            {
                AgregarErrores(reserva, esNueva: true);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int idReserva = _reservaService.Agregar(reserva, idUsuario);

                    if (idReserva > 0)
                    {
                        TempData["Mensaje"] = $"Reserva #{idReserva} registrada correctamente.";
                        return RedirectToAction(nameof(Detalles), new { id = idReserva });
                    }

                    ViewBag.Error = "La habitación acaba de ser reservada por otra persona en esas fechas. Elija otras fechas u otra habitación.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al guardar la reserva: " + ex.Message;
                }
            }

            await CargarDatosFormulario(reserva);
            return View(reserva);
        }

  

        [Authorize(Roles = Roles.Personal)]
        public async Task<IActionResult> Editar(int id)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (!reserva.Estado)
            {
                TempData["Mensaje"] = "Una reserva cancelada no se puede modificar.";
                return RedirectToAction(nameof(Detalles), new { id });
            }

            await CargarDatosFormulario(reserva);
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Personal)]
        public async Task<IActionResult> Editar(Reserva reserva)
        {
            Reserva? actual = _reservaService.ObtenerPorId(reserva.IdReserva);

            if (actual == null)
            {
                return NotFound();
            }

        
            reserva.IdUsuario = actual.IdUsuario;

            if (ModelState.IsValid)
            {
                AgregarErrores(reserva, esNueva: false);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (_reservaService.Editar(reserva))
                    {
                        TempData["Mensaje"] = "La reserva se actualizó correctamente.";
                        return RedirectToAction(nameof(Detalles), new { id = reserva.IdReserva });
                    }

                    ViewBag.Error = "No se pudo actualizar la reserva: la habitación ya no está disponible en esas fechas.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al actualizar la reserva: " + ex.Message;
                }
            }

            reserva.MontoTotal = actual.MontoTotal;
            reserva.TotalPagado = actual.TotalPagado;
            reserva.HuespedTexto = actual.HuespedTexto;
            await CargarDatosFormulario(reserva);
            return View(reserva);
        }

  

        // GET: Reserva/Cancelar/5
        public IActionResult Cancelar(int id)
        {
            Reserva? reserva = ObtenerReservaVisible(id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (!PuedeCancelar(reserva, out string? motivo))
            {
                TempData["Mensaje"] = motivo;
                return RedirectToAction(nameof(Detalles), new { id });
            }

            return View(reserva);
        }

        [HttpPost, ActionName("Cancelar")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarCancelar(int id)
        {
            Reserva? reserva = ObtenerReservaVisible(id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (!PuedeCancelar(reserva, out string? motivo))
            {
                TempData["Mensaje"] = motivo;
                return RedirectToAction(nameof(Detalles), new { id });
            }

            if (_reservaService.Cancelar(id))
            {
                TempData["Mensaje"] = reserva.TotalPagado > 0
                    ? $"La reserva #{id} se canceló. Tiene {reserva.TotalPagado.Colones()} en pagos aplicados: un administrador debe anularlos o gestionar el reembolso."
                    : $"La reserva #{id} se canceló correctamente.";

                return EsPersonal
                    ? RedirectToAction(nameof(Detalles), new { id })
                    : RedirectToAction(nameof(MisReservas));
            }

            ViewBag.Error = "No se pudo cancelar la reserva (puede que ya esté cancelada).";
            return View(reserva);
        }


        private Reserva? ObtenerReservaVisible(int idReserva)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(idReserva);

            if (reserva == null)
            {
                return null;
            }

            // El personal puede consultar cualquier reserva.
            if (EsPersonal)
            {
                return reserva;
            }

            // Obtener las reservas asociadas al usuario que inició sesión.
            List<Reserva> misReservas =
                _reservaService.ObtenerPorUsuario(IdUsuarioActual);

            // Comprobar que la reserva pertenece al cliente.
            return misReservas.Any(r => r.IdReserva == idReserva)
                ? reserva
                : null;
        }


        private bool PuedeCancelar(Reserva reserva, out string? motivo)
        {
            motivo = null;
            DateTime hoy = _fechaService.ObtenerFechaActual().Date;

            if (!reserva.Estado)
            {
                motivo = "La reserva ya está cancelada.";
            }
            else if (reserva.FechaSalida.Date <= hoy)
            {
                motivo = "La estadía ya finalizó; no se puede cancelar.";
            }
            else if (!EsPersonal && reserva.FechaEntrada.Date <= hoy)
            {
                motivo = "Solo puede cancelar su reserva antes de la fecha de entrada. Comuníquese con recepción.";
            }

            return motivo == null;
        }

        private void AgregarErrores(Reserva reserva, bool esNueva)
        {
            foreach (var (campo, mensaje) in _reservaService.Validar(reserva, esNueva))
            {
                ModelState.AddModelError(campo, mensaje);
            }
        }

        private async Task CargarDatosFormulario(Reserva reserva)
        {
            Habitacion? habitacionActual = _habitacionService.ObtenerPorId(reserva.IdHabitacion);
            ViewBag.Habitacion = habitacionActual;
            ViewBag.Paquetes = _reservaService.ObtenerPaquetesVigentes(reserva.IdPaquete);
            ViewBag.EsPersonal = EsPersonal;

            if (EsPersonal)
            {
              
                List<Habitacion> habitaciones = _habitacionService.ObtenerActivas();
                if (habitacionActual != null && habitaciones.All(h => h.IdHabitacion != habitacionActual.IdHabitacion))
                {
                    habitaciones.Add(habitacionActual);
                }
                ViewBag.Habitaciones = habitaciones;

                IList<IdentityUser> clientes = await _userManager.GetUsersInRoleAsync(Roles.Cliente);
                ViewBag.Huespedes = clientes
                    .OrderBy(u => u.Email)
                    .Select(u => new HuespedOpcion { Id = u.Id, Texto = u.Email ?? u.UserName ?? u.Id })
                    .ToList();
            }
        }
    }
}
