using System.Data;
using Hotel.Data;
using Hotel.Data.Entidades;
using Hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Services
{
    public class PagoService
    {
        private readonly HotelContext _contexto;
        private readonly FechaService _fechaService;
        private readonly UsuarioHotelService _usuarioService;
        private readonly ReservaService _reservaService;

        public PagoService(HotelContext contexto, FechaService fechaService,
            UsuarioHotelService usuarioService, ReservaService reservaService)
        {
            _contexto = contexto;
            _fechaService = fechaService;
            _usuarioService = usuarioService;
            _reservaService = reservaService;
        }

     

        public List<Pago> ObtenerPorReserva(int idReserva)
        {
            return Convertir(_contexto.Pagos
                .Where(p => p.IdReserva == idReserva)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPago)
                .ToList());
        }

        public Pago? ObtenerPorId(int idPago)
        {
            PagoEntidad? entidad = _contexto.Pagos.Find(idPago);
            return entidad == null ? null : Convertir(new List<PagoEntidad> { entidad })[0];
        }

     
        public List<Pago> Buscar(HistorialPagosViewModel filtro)
        {
            IQueryable<PagoEntidad> consulta = _contexto.Pagos;

            if (filtro.IdReserva.HasValue)
            {
                consulta = consulta.Where(p => p.IdReserva == filtro.IdReserva.Value);
            }

            if (filtro.Desde.HasValue)
            {
                DateTime desde = filtro.Desde.Value.Date;
                consulta = consulta.Where(p => p.FechaPago >= desde);
            }

            if (filtro.Hasta.HasValue)
            {
                DateTime hastaExclusivo = filtro.Hasta.Value.Date.AddDays(1);
                consulta = consulta.Where(p => p.FechaPago < hastaExclusivo);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Metodo))
            {
                consulta = consulta.Where(p => p.MetodoPago == filtro.Metodo);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Estado))
            {
                consulta = consulta.Where(p => p.Estado == filtro.Estado);
            }

            return Convertir(consulta
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPago)
                .ToList());
        }


        public List<(string Campo, string Mensaje)> ValidarRegistro(RegistrarPagoViewModel pago)
        {
            List<(string Campo, string Mensaje)> errores = new();
            string prefijo = nameof(PagosReservaViewModel.NuevoPago) + ".";

            ReservaEntidad? reserva = _contexto.Reservas.Find(pago.IdReserva);

            if (reserva == null)
            {
                errores.Add((string.Empty, "La reserva no existe."));
                return errores;
            }

            if (!reserva.Estado)
            {
                errores.Add((string.Empty, "No se pueden registrar pagos en una reserva cancelada."));
                return errores;
            }

          
            if (!string.IsNullOrEmpty(pago.MetodoPago) && !MetodosPago.Todos.Contains(pago.MetodoPago))
            {
                errores.Add((prefijo + nameof(pago.MetodoPago), "El método de pago no es válido."));
            }

         
            if (pago.FechaPago.HasValue)
            {
                DateTime fecha = pago.FechaPago.Value.Date;
                DateTime hoy = _fechaService.ObtenerFechaActual().Date;

                if (fecha > hoy)
                {
                    errores.Add((prefijo + nameof(pago.FechaPago), "La fecha del pago no puede ser futura."));
                }
                else if (fecha < reserva.FechaRegistro.Date)
                {
                    errores.Add((prefijo + nameof(pago.FechaPago),
                        $"La fecha del pago no puede ser anterior al registro de la reserva ({reserva.FechaRegistro:dd/MM/yyyy})."));
                }
            }

         
            if (pago.Monto.HasValue)
            {
                decimal saldo = reserva.MontoTotal - _reservaService.ObtenerTotalPagado(reserva.IdReserva);

                if (pago.Monto.Value <= 0)
                {
                    errores.Add((prefijo + nameof(pago.Monto), "El monto debe ser mayor a 0."));
                }
                else if (saldo <= 0)
                {
                    errores.Add((prefijo + nameof(pago.Monto), "La reserva ya está pagada por completo."));
                }
                else if (pago.Monto.Value > saldo)
                {
                    errores.Add((prefijo + nameof(pago.Monto),
                        $"El monto no puede superar el saldo pendiente ({saldo.Colones()})."));
                }
            }

            return errores;
        }


        public int Registrar(RegistrarPagoViewModel pago, string idUsuarioSesion)
        {
            int idUsuario = _usuarioService.AsegurarRegistro(idUsuarioSesion);
            DateTime ahora = _fechaService.ObtenerFechaActual();

          
            using var transaccion = _contexto.Database.BeginTransaction(IsolationLevel.Serializable);

            ReservaEntidad? reserva = _contexto.Reservas.Find(pago.IdReserva);
            decimal saldo = reserva == null ? 0 : reserva.MontoTotal - _reservaService.ObtenerTotalPagado(pago.IdReserva);

            if (reserva == null || !reserva.Estado || pago.Monto!.Value > saldo)
            {
                transaccion.Rollback();
                return 0;
            }

            DateTime fecha = pago.FechaPago!.Value.Date;

            PagoEntidad entidad = new()
            {
                IdReserva = pago.IdReserva,
                IdUsuarioRegistro = idUsuario,
                Monto = pago.Monto.Value,
             
                FechaPago = fecha == ahora.Date ? ahora : fecha,
                MetodoPago = pago.MetodoPago!,
                Estado = EstadosPago.Aplicado
            };

            _contexto.Pagos.Add(entidad);
            _contexto.SaveChanges();
            transaccion.Commit();

            return entidad.IdPago;
        }


        public string? Anular(int idPago, string motivo, string idUsuarioSesion)
        {
            PagoEntidad? pago = _contexto.Pagos.Find(idPago);

            if (pago == null)
            {
                return "El pago no existe.";
            }

            if (pago.Estado == EstadosPago.Anulado || _contexto.AnulacionesPago.Any(a => a.IdPago == idPago))
            {
                return "Este pago ya fue anulado.";
            }
            
            int idUsuario = _usuarioService.AsegurarRegistro(idUsuarioSesion);

            using var transaccion = _contexto.Database.BeginTransaction();

            _contexto.AnulacionesPago.Add(new AnulacionPagoEntidad
            {
                IdPago = idPago,
                IdUsuario = idUsuario,
                Motivo = motivo.Trim(),
                FechaAnulacion = _fechaService.ObtenerFechaActual()
            });

            pago.Estado = EstadosPago.Anulado;

            _contexto.SaveChanges();
            transaccion.Commit();

            return null; 
        }


        private List<Pago> Convertir(List<PagoEntidad> entidades)
        {
            if (entidades.Count == 0)
            {
                return new List<Pago>();
            }

            List<int> idsPago = entidades.Select(p => p.IdPago).ToList();

            Dictionary<int, AnulacionPagoEntidad> anulaciones = _contexto.AnulacionesPago
                .Where(a => idsPago.Contains(a.IdPago))
                .ToDictionary(a => a.IdPago);

            Dictionary<int, string> nombres = _usuarioService.ObtenerNombres(
                entidades.Select(p => p.IdUsuarioRegistro)
                    .Concat(anulaciones.Values.Select(a => a.IdUsuario)));

            return entidades.Select(p =>
            {
                anulaciones.TryGetValue(p.IdPago, out AnulacionPagoEntidad? anulacion);

                return new Pago
                {
                    IdPago = p.IdPago,
                    IdReserva = p.IdReserva,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago,
                    MetodoPago = p.MetodoPago,
                    Estado = p.Estado,
                    RegistradoPor = nombres.TryGetValue(p.IdUsuarioRegistro, out string? r)
                                    ? r
                                    : p.IdUsuarioRegistro.ToString(),
                    AnuladoPor = anulacion == null ? null
                                : nombres.TryGetValue(anulacion.IdUsuario, out string? a)
                                    ? a
                                    : anulacion.IdUsuario.ToString(),
                    MotivoAnulacion = anulacion?.Motivo,
                    FechaAnulacion = anulacion?.FechaAnulacion
                };
            }).ToList();
        }
    }
}
