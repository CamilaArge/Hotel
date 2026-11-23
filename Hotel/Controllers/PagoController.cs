using System.Security.Claims;
using Hotel.Models;
using Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{

    [Authorize(Roles = Roles.Personal)]
    public class PagoController : Controller
    {
        private readonly PagoService _pagoService;
        private readonly ReservaService _reservaService;
        private readonly FechaService _fechaService;

        public PagoController(PagoService pagoService, ReservaService reservaService, FechaService fechaService)
        {
            _pagoService = pagoService;
            _reservaService = reservaService;
            _fechaService = fechaService;
        }

        private string IdUsuarioActual => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

   
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Index(HistorialPagosViewModel filtro)
        {
            if (filtro.Desde.HasValue && filtro.Hasta.HasValue && filtro.Desde.Value.Date > filtro.Hasta.Value.Date)
            {
                ViewBag.Error = "La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.";
                return View(filtro);
            }

            filtro.Resultados = _pagoService.Buscar(filtro);
            return View(filtro);
        }

     
        public IActionResult Buscar(int? idReserva)
        {
            if (idReserva.HasValue)
            {
                if (_reservaService.ObtenerPorId(idReserva.Value) != null)
                {
                    return RedirectToAction(nameof(Reserva), new { id = idReserva.Value });
                }

                ViewBag.Error = $"No existe la reserva #{idReserva.Value}.";
            }

            List<Reserva> pendientes = _reservaService.Buscar(new FiltroReservas())
                .Where(r => r.Estado && r.Saldo > 0)
                .OrderBy(r => r.FechaEntrada)
                .ToList();

            return View(pendientes);
        }

        // GET: Pago/Reserva
        public IActionResult Reserva(int id)
        {
            PagosReservaViewModel? modelo = CrearModelo(id);
            return modelo == null ? NotFound() : View(modelo);
        }

        // POST: Pago/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar([Bind(Prefix = nameof(PagosReservaViewModel.NuevoPago))] RegistrarPagoViewModel nuevoPago)
        {
            PagosReservaViewModel? modelo = CrearModelo(nuevoPago.IdReserva);

            if (modelo == null)
            {
                return NotFound();
            }

            modelo.NuevoPago = nuevoPago;

            foreach (var (campo, mensaje) in _pagoService.ValidarRegistro(nuevoPago))
            {
                ModelState.AddModelError(campo, mensaje);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int idPago = _pagoService.Registrar(nuevoPago, IdUsuarioActual);

                    if (idPago > 0)
                    {
                        TempData["Mensaje"] = $"Pago PAG-{idPago:0000} por {nuevoPago.Monto!.Value.Colones()} registrado correctamente.";
                        return RedirectToAction(nameof(Reserva), new { id = nuevoPago.IdReserva });
                    }

                    ViewBag.Error = "El saldo de la reserva cambió mientras se registraba el pago. Revise el saldo e intente de nuevo.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al registrar el pago: " + ex.Message;
                }

                modelo = CrearModelo(nuevoPago.IdReserva)!;
                modelo.NuevoPago = nuevoPago;
            }

            return View(nameof(Reserva), modelo);
        }

        // GET: Pago/Anular/5
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Anular(int id)
        {
            Pago? pago = _pagoService.ObtenerPorId(id);

            if (pago == null)
            {
                return NotFound();
            }

            if (pago.EstaAnulado)
            {
                TempData["Mensaje"] = "Este pago ya fue anulado.";
                return RedirectToAction(nameof(Reserva), new { id = pago.IdReserva });
            }

            return View(new AnularPagoViewModel { IdPago = id, Pago = pago });
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Anular(AnularPagoViewModel modelo)
        {
            Pago? pago = _pagoService.ObtenerPorId(modelo.IdPago);

            if (pago == null)
            {
                return NotFound();
            }

            modelo.Pago = pago;

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            try
            {
                string? error = _pagoService.Anular(modelo.IdPago, modelo.Motivo, IdUsuarioActual);

                if (error == null)
                {
                    TempData["Mensaje"] = $"El pago PAG-{pago.IdPago:0000} fue anulado. El saldo de la reserva se recalculó.";
                    return RedirectToAction(nameof(Reserva), new { id = pago.IdReserva });
                }

                ViewBag.Error = error;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al anular el pago: " + ex.Message;
            }

            return View(modelo);
        }

        private PagosReservaViewModel? CrearModelo(int idReserva)
        {
            Reserva? reserva = _reservaService.ObtenerPorId(idReserva);

            if (reserva == null)
            {
                return null;
            }

            return new PagosReservaViewModel
            {
                Reserva = reserva,
                Pagos = _pagoService.ObtenerPorReserva(idReserva),
                NuevoPago = new RegistrarPagoViewModel
                {
                    IdReserva = idReserva,
                    FechaPago = _fechaService.ObtenerFechaActual().Date
                }
            };
        }
    }
}
