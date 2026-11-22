using System.Security.Claims;
using Hotel.Models;
using Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Controllers
{
   
    [Authorize]
    public class ReservaController : Controller
    {
        private readonly ReservaService _reservaService;
        private readonly HabitacionService _habitacionService;

        public ReservaController(ReservaService reservaService, HabitacionService habitacionService)
        {
            _reservaService = reservaService;
            _habitacionService = habitacionService;
        }

        // GET: Reserva/ObtenerHabitaciones  
        [Authorize(Roles = Roles.Cliente)]
        public IActionResult ObtenerHabitaciones()
        {
            List<Habitacion> habitaciones = _habitacionService.ObtenerActivas();
            return View(habitaciones);
        }

        // GET: Reserva/ObtenerReservas?idHabitacion=5
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult ObtenerReservas(int idHabitacion)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(idHabitacion);

            if (habitacion == null)
            {
                return NotFound();
            }

            ViewBag.Habitacion = habitacion;
            List<Reserva> reservas = _reservaService.ObtenerPorHabitacion(idHabitacion);
            return View(reservas);
        }

        // GET: Reserva/Detalles?idReservacion=5
        [Authorize(Roles = Roles.Cliente)]
        public IActionResult Detalles(int idReservacion)
        {
            Reserva? reserva = ObtenerReservaVisible(idReservacion);

            if (reserva == null)
            {
                TempData["Mensaje"] = "Estimado usuario, no se ha encontrado la reservación, favor realice una.";
                return RedirectToAction(nameof(ObtenerHabitaciones));
            }

            return View(reserva);
        }

        // GET
        [Authorize(Roles = Roles.Cliente)]
        public IActionResult DetallesParcial(int idReservacion)
        {
            Reserva? reserva = ObtenerReservaVisible(idReservacion);

            if (reserva == null)
            {
                return Content("");
            }

            return PartialView("_DetallesParcial", reserva);
        }

        // GET: Reserva/AgregarReserva?idHabitacion=5
        [Authorize(Roles = Roles.Cliente)]
        public IActionResult AgregarReserva(int idHabitacion)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(idHabitacion);

            if (habitacion == null)
            {
                return NotFound();
            }

            ViewBag.Habitacion = habitacion;
            return View(new Reserva { IdHabitacion = idHabitacion });
        }

        // POST: Reserva/AgregarReserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Cliente)]
        public IActionResult AgregarReserva(Reserva reserva)
        {
            AgregarErroresDeNegocio(reserva, esNueva: true);

            if (ModelState.IsValid)
            {
                try
                {
                    string? idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    int idReserva = _reservaService.Agregar(reserva, idUsuario);

                    if (idReserva > 0)
                    {
                        return RedirectToAction(nameof(Detalles), new { idReservacion = idReserva });
                    }

                    ViewBag.Error = "No se pudo guardar la reserva.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al guardar la reserva: " + ex.Message;
                }
            }

            ViewBag.Habitacion = _habitacionService.ObtenerPorId(reserva.IdHabitacion);
            return View(reserva);
        }

        // GET: Reserva/Editar/5
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Editar(int id)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (!reserva.Estado)
            {
                TempData["Mensaje"] = "Una reserva cancelada no se puede modificar.";
                return RedirectToAction(nameof(ObtenerReservas), new { idHabitacion = reserva.IdHabitacion });
            }

            CargarHabitaciones(reserva.IdHabitacion);
            return View(reserva);
        }

        // POST: Reserva/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Editar(Reserva reserva)
        {
            AgregarErroresDeNegocio(reserva, esNueva: false);

            if (ModelState.IsValid)
            {
                try
                {
                    if (_reservaService.Editar(reserva))
                    {
                        TempData["Mensaje"] = "La reserva se actualizó correctamente.";
                        return RedirectToAction(nameof(ObtenerReservas), new { idHabitacion = reserva.IdHabitacion });
                    }

                    ViewBag.Error = "No se pudo actualizar la reserva.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al actualizar la reserva: " + ex.Message;
                }
            }

            CargarHabitaciones(reserva.IdHabitacion);
            return View(reserva);
        }

        // GET: Reserva/Cancelar/5
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Cancelar(int id)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reserva/Cancelar/5
        [HttpPost, ActionName("Cancelar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult ConfirmarCancelar(int id)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (_reservaService.Cancelar(id))
            {
                TempData["Mensaje"] = "La reserva se canceló correctamente.";
                return RedirectToAction(nameof(ObtenerReservas), new { idHabitacion = reserva.IdHabitacion });
            }

            ViewBag.Error = "No se pudo cancelar la reserva (puede que ya esté cancelada).";
            return View(reserva);
        }

     
        private Reserva? ObtenerReservaVisible(int idReservacion)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(idReservacion);

            if (reserva == null)
            {
                return null;
            }

            string? idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return reserva.IdUsuario == idUsuario ? reserva : null;
        }

      
        private void AgregarErroresDeNegocio(Reserva reserva, bool esNueva)
        {
            foreach (var (campo, mensaje) in _reservaService.Validar(reserva, esNueva))
            {
                ModelState.AddModelError(campo, mensaje);
            }
        }

        private void CargarHabitaciones(int idSeleccionada)
        {
            ViewBag.Habitaciones = new SelectList(
                _habitacionService.ObtenerActivas(), "IdHabitacion", "Nombre", idSeleccionada);
        }
    }
}
