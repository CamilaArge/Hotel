using HotelLosPatitos.Models;
using HotelLosPatitos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLosPatitos.Controllers
{
  
    [Authorize(Roles = Roles.Administrador)]
    public class HabitacionController : Controller
    {
        private readonly HabitacionService _habitacionService;

        public HabitacionController(HabitacionService habitacionService)
        {
            _habitacionService = habitacionService;
        }

        // GET: Habitacion/ObtenerHabitaciones
        public IActionResult ObtenerHabitaciones()
        {
            List<Habitacion> habitaciones = _habitacionService.ObtenerTodas();
            return View(habitaciones);
        }

        // GET: Habitacion/Detalles/5
        public IActionResult Detalles(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            return View(habitacion);
        }

        // GET: Habitacion/AgregarHabitacion
        public IActionResult AgregarHabitacion()
        {
            return View(new Habitacion());
        }

        // POST: Habitacion/AgregarHabitacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarHabitacion(Habitacion habitacion)
        {
            if (!string.IsNullOrWhiteSpace(habitacion.Codigo) && _habitacionService.ExisteCodigo(habitacion.Codigo))
            {
                ModelState.AddModelError(nameof(Habitacion.Codigo), "Ya existe una habitación con ese código.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (_habitacionService.Agregar(habitacion))
                    {
                        TempData["Mensaje"] = "La habitación se agregó correctamente.";
                        return RedirectToAction(nameof(ObtenerHabitaciones));
                    }

                    ViewBag.Error = "No se pudo guardar la habitación.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al guardar la habitación: " + (ex.InnerException?.Message ?? ex.Message);
                }
            }

            return View(habitacion);
        }

        // GET: Habitacion/Editar/5
        public IActionResult Editar(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            return View(habitacion);
        }

        // POST: Habitacion/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Habitacion habitacion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (_habitacionService.Editar(habitacion))
                    {
                        TempData["Mensaje"] = "La habitación se actualizó correctamente.";
                        return RedirectToAction(nameof(ObtenerHabitaciones));
                    }

                    ViewBag.Error = "No se pudo actualizar la habitación.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al actualizar la habitación: " + (ex.InnerException?.Message ?? ex.Message);
                }
            }

            return View(habitacion);
        }

        // GET: Habitacion/Eliminar/5
        public IActionResult Eliminar(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            return View(habitacion);
        }

        // POST: Habitacion/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarEliminar(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);

            if (habitacion == null)
            {
                return NotFound();
            }

         
            if (_habitacionService.TieneReservas(id))
            {
                ViewBag.Error = "No se puede eliminar la habitación porque tiene reservas. Puede desactivarla desde Editar.";
                return View(habitacion);
            }

            try
            {
                if (_habitacionService.Eliminar(id))
                {
                    TempData["Mensaje"] = "La habitación se eliminó correctamente.";
                    return RedirectToAction(nameof(ObtenerHabitaciones));
                }

                ViewBag.Error = "No se pudo eliminar la habitación.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al eliminar la habitación: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return View(habitacion);
        }
    }
}
