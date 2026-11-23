using Hotel.Models;
using Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Controllers
{

    [Authorize(Roles = Roles.Personal)]
    public class HabitacionController : Controller
    {
        private readonly HabitacionService _habitacionService;
        private readonly TipoHabitacionService _tipoHabitacionService;

        public HabitacionController(HabitacionService habitacionService, TipoHabitacionService tipoHabitacionService)
        {
            _habitacionService = habitacionService;
            _tipoHabitacionService = tipoHabitacionService;
        }

        // GET: Habitacion/ObtenerHabitaciones
        public IActionResult ObtenerHabitaciones()
        {
            return View(_habitacionService.ObtenerTodas());
        }

        // GET: Habitacion/Detalles/5
        public IActionResult Detalles(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);
            return habitacion == null ? NotFound() : View(habitacion);
        }

       

        [Authorize(Roles = Roles.Administrador)]
        public IActionResult AgregarHabitacion()
        {
            CargarTipos(0);
            return View(new Habitacion());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult AgregarHabitacion(Habitacion habitacion)
        {
            // HABITACION-CP-006: código duplicado
            if (!string.IsNullOrWhiteSpace(habitacion.Codigo) && _habitacionService.ExisteCodigo(habitacion.Codigo))
            {
                ModelState.AddModelError(nameof(Habitacion.Codigo), "Ya existe una habitación con ese código.");
            }

            if (habitacion.IdTipoHabitacion > 0 && !_tipoHabitacionService.ExisteActivo(habitacion.IdTipoHabitacion))
            {
                ModelState.AddModelError(nameof(Habitacion.IdTipoHabitacion), "El tipo de habitación seleccionado no es válido.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (_habitacionService.Agregar(habitacion))
                    {
                        TempData["Mensaje"] = "La habitación se registró correctamente.";
                        return RedirectToAction(nameof(ObtenerHabitaciones));
                    }

                    ViewBag.Error = "No se pudo guardar la habitación.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al guardar la habitación: " + ex.Message;
                }
            }

            CargarTipos(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

      

        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Editar(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            CargarTipos(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Editar(Habitacion habitacion)
        {
            Habitacion? actual = _habitacionService.ObtenerPorId(habitacion.IdHabitacion);

            if (actual == null)
            {
                return NotFound();
            }

           
            habitacion.Codigo = actual.Codigo;
            ModelState.Remove(nameof(Habitacion.Codigo));

            if (habitacion.IdTipoHabitacion > 0 &&
                habitacion.IdTipoHabitacion != actual.IdTipoHabitacion &&
                !_tipoHabitacionService.ExisteActivo(habitacion.IdTipoHabitacion))
            {
                ModelState.AddModelError(nameof(Habitacion.IdTipoHabitacion), "El tipo de habitación seleccionado no es válido.");
            }

            if (actual.Estado && !habitacion.Estado && _habitacionService.TieneReservasVigentes(habitacion.IdHabitacion))
            {
                ModelState.AddModelError(nameof(Habitacion.Estado),
                    "No se puede desactivar la habitación porque tiene reservas en curso o futuras.");
            }

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
                    ViewBag.Error = "Error al actualizar la habitación: " + ex.Message;
                }
            }

            CargarTipos(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

     
        // GET: Habitacion/ActualizarEstado/5
        public IActionResult ActualizarEstado(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);
            return habitacion == null ? NotFound() : View(habitacion);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarEstado(int id, bool activa)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            if (habitacion.Estado == activa)
            {
                TempData["Mensaje"] = $"La habitación {habitacion.Codigo} ya estaba en ese estado.";
                return RedirectToAction(nameof(ObtenerHabitaciones));
            }

            if (!activa && _habitacionService.TieneReservasVigentes(id))
            {
                ViewBag.Error = "No se puede pasar a mantenimiento: la habitación tiene reservas en curso o futuras. " +
                                "Primero reubique o cancele esas reservas.";
                return View(habitacion);
            }

            if (_habitacionService.CambiarEstado(id, activa))
            {
                TempData["Mensaje"] = $"La habitación {habitacion.Codigo} quedó en estado " +
                                      (activa ? EstadosHabitacion.Disponible : EstadosHabitacion.Mantenimiento) + ".";
                return RedirectToAction(nameof(ObtenerHabitaciones));
            }

            ViewBag.Error = "No se pudo actualizar el estado.";
            return View(habitacion);
        }


        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Eliminar(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            if (_habitacionService.TieneReservas(id))
            {
                ViewBag.Error = "Esta habitación tiene reservas asociadas y no se puede eliminar. Puede desactivarla desde Editar.";
                ViewBag.Bloqueada = true;
            }

            return View(habitacion);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult ConfirmarEliminar(int id)
        {
            Habitacion? habitacion = _habitacionService.ObtenerPorId(id);

            if (habitacion == null)
            {
                return NotFound();
            }

         
            if (_habitacionService.TieneReservas(id))
            {
                ViewBag.Error = "Esta habitación tiene reservas asociadas y no se puede eliminar. Puede desactivarla desde Editar.";
                ViewBag.Bloqueada = true;
                return View(habitacion);
            }

            if (_habitacionService.Eliminar(id))
            {
                TempData["Mensaje"] = "La habitación se eliminó correctamente.";
                return RedirectToAction(nameof(ObtenerHabitaciones));
            }

            ViewBag.Error = "No se pudo eliminar la habitación.";
            return View(habitacion);
        }

        private void CargarTipos(int idSeleccionado)
        {
            ViewBag.Tipos = new SelectList(
                _tipoHabitacionService.ObtenerParaSelect(idSeleccionado),
                "IdTipoHabitacion", "Nombre", idSeleccionado);
        }
    }
}
