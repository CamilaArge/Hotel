using HotelLosPatitos.Models;
using HotelLosPatitos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLosPatitos.Controllers
{
    // Hay que iniciar sesión para usar el módulo; cada acción indica qué rol la puede usar
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

        // GET: Reserva/ObtenerHabitaciones  (habitaciones activas disponibles para reservar)
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
            Reserva? reserva = _reservaService.ObtenerPorId(idReservacion);

            if (reserva == null)
            {
                TempData["Mensaje"] = "Estimado usuario, no se ha encontrado la reservación, favor realice una.";
                return RedirectToAction(nameof(ObtenerHabitaciones));
            }

            return View(reserva);
        }

        // GET: Reserva/DetallesParcial?idReservacion=5  (se usa desde el buscador con AJAX)
        [Authorize(Roles = Roles.Cliente)]
        public IActionResult DetallesParcial(int idReservacion)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(idReservacion);

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
            ValidarFechas(reserva);

            if (ModelState.IsValid)
            {
                try
                {
                    int idReserva = _reservaService.Agregar(reserva);

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

            return View(reserva);
        }

        // POST: Reserva/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Editar(Reserva reserva)
        {
            ValidarFechas(reserva);

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

            return View(reserva);
        }

        // GET: Reserva/Eliminar/5
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Eliminar(int id)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reserva/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult ConfirmarEliminar(int id)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            int idHabitacion = reserva.IdHabitacion;

            if (_reservaService.Eliminar(id))
            {
                TempData["Mensaje"] = "La reserva se eliminó correctamente.";
                return RedirectToAction(nameof(ObtenerReservas), new { idHabitacion });
            }

            ViewBag.Error = "No se pudo eliminar la reserva.";
            return View(reserva);
        }

        private void ValidarFechas(Reserva reserva)
        {
            if (reserva.FechaFinReserva.Date <= reserva.FechaInicioReserva.Date)
            {
                ModelState.AddModelError(nameof(Reserva.FechaFinReserva),
                    "La fecha de salida debe ser posterior a la fecha de entrada.");
            }
        }
    }
}
