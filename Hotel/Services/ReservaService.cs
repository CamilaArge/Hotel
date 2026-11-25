using System.Data;
using Hotel.Data;
using Hotel.Data.Entidades;
using Hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Services
{
    public class ReservaService
    {
        private readonly HotelContext _contexto;
        private readonly FechaService _fechaService;
        private readonly UsuarioHotelService _usuarioService;

        public ReservaService(HotelContext contexto, FechaService fechaService, UsuarioHotelService usuarioService)
        {
            _contexto = contexto;
            _fechaService = fechaService;
            _usuarioService = usuarioService;
        }

     
        public List<Reserva> Buscar(FiltroReservas filtro)
        {
            IQueryable<ReservaEntidad> consulta = _contexto.Reservas;

            if (filtro.Numero.HasValue)
            {
                consulta = consulta.Where(r => r.IdReserva == filtro.Numero.Value);
            }

            if (filtro.IdHabitacion.HasValue)
            {
                consulta = consulta.Where(r => r.IdHabitacion == filtro.IdHabitacion.Value);
            }

            List<Reserva> reservas = CompletarInfo(consulta
                .OrderByDescending(r => r.FechaEntrada)
                .ThenByDescending(r => r.IdReserva)
                .ToList());

          
            if (!string.IsNullOrWhiteSpace(filtro.Huesped))
            {
                string texto = filtro.Huesped.Trim();
                reservas = reservas
                    .Where(r => r.HuespedTexto.Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(filtro.Estado))
            {
                reservas = reservas.Where(r => r.EstadoTexto == filtro.Estado).ToList();
            }

            return reservas;
        }

        public List<Reserva> ObtenerPorHabitacion(int idHabitacion)
        {
            return Buscar(new FiltroReservas { IdHabitacion = idHabitacion });
        }

        public List<Reserva> ObtenerPorUsuario(string idUsuario)
        {
            int idUsuarioInterno = _usuarioService.AsegurarRegistro(idUsuario);

            return CompletarInfo(_contexto.Reservas
                .Where(r => r.IdUsuario == idUsuarioInterno)
                .OrderByDescending(r => r.FechaEntrada)
                .ToList());
        }

        public Reserva? ObtenerPorId(int id)
        {
            ReservaEntidad? entidad = _contexto.Reservas.Find(id);
            return entidad == null ? null : CompletarInfo(new List<ReservaEntidad> { entidad })[0];
        }

    
        public List<PaqueteOpcion> ObtenerPaquetesVigentes(int? incluirId = null)
        {
            DateTime hoy = _fechaService.ObtenerFechaActual().Date;

            return _contexto.Paquetes
                .Where(p => (p.Activo && p.FechaFin >= hoy) || p.IdPaquete == incluirId)
                .OrderBy(p => p.Nombre)
                .Select(p => new PaqueteOpcion
                {
                    IdPaquete = p.IdPaquete,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin
                })
                .ToList();
        }

        // Total pagado
        public decimal ObtenerTotalPagado(int idReserva)
        {
            return _contexto.Pagos
                .Where(p => p.IdReserva == idReserva && p.Estado == EstadosPago.Aplicado)
                .Sum(p => (decimal?)p.Monto) ?? 0m;
        }

     
        public bool HayConflicto(int idHabitacion, DateTime entrada, DateTime salida, int idReservaExcluir = 0)
        {
            DateTime e = entrada.Date;
            DateTime s = salida.Date;

            return _contexto.Reservas.Any(r =>
                r.IdHabitacion == idHabitacion &&
                r.IdReserva != idReservaExcluir &&
                r.Estado &&
                r.FechaEntrada < s &&
                r.FechaSalida > e);
        }

    
        public List<(string Campo, string Mensaje)> Validar(Reserva reserva, bool esNueva)
        {
            List<(string Campo, string Mensaje)> errores = new();
            DateTime hoy = _fechaService.ObtenerFechaActual().Date;

            ReservaEntidad? original = null;
            if (!esNueva)
            {
                original = _contexto.Reservas.Find(reserva.IdReserva);
                if (original == null || !original.Estado)
                {
                    errores.Add((string.Empty, "La reserva no existe o ya está cancelada."));
                    return errores;
                }
            }

            HabitacionEntidad? habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion);
            if (habitacion == null)
            {
                errores.Add((nameof(Reserva.IdHabitacion), "La habitación seleccionada no existe."));
                return errores;
            }

            bool cambiaHabitacion = original == null || original.IdHabitacion != reserva.IdHabitacion;
            bool cambiaEntrada = original == null || original.FechaEntrada.Date != reserva.FechaEntrada.Date;
            bool cambiaFechas = cambiaEntrada || original!.FechaSalida.Date != reserva.FechaSalida.Date;
            bool cambiaPaquete = original == null || original.IdPaquete != reserva.IdPaquete;

            if (!habitacion.Estado && cambiaHabitacion)
            {
                errores.Add((nameof(Reserva.IdHabitacion), "La habitación no está disponible para reservas (inactiva o en mantenimiento)."));
            }

         
            if (reserva.FechaSalida.Date <= reserva.FechaEntrada.Date)
            {
                errores.Add((nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior a la fecha de entrada."));
                return errores;
            }

            if (cambiaEntrada && reserva.FechaEntrada.Date < hoy)
            {
                errores.Add((nameof(Reserva.FechaEntrada), "La fecha de entrada no puede ser anterior a hoy."));
            }

         
            if (reserva.CantidadPersonas > habitacion.CantidadHuespedes)
            {
                errores.Add((nameof(Reserva.CantidadPersonas),
                    $"La habitación permite como máximo {habitacion.CantidadHuespedes} huésped(es)."));
            }

          
            PaqueteEntidad? paquete = null;
            if (reserva.IdPaquete.HasValue)
            {
                paquete = _contexto.Paquetes.Find(reserva.IdPaquete.Value);

                if (paquete == null)
                {
                    errores.Add((nameof(Reserva.IdPaquete), "El paquete seleccionado no existe."));
                }
                else
                {
                    if (cambiaPaquete && !paquete.Activo)
                    {
                        errores.Add((nameof(Reserva.IdPaquete), "El paquete seleccionado no está disponible."));
                    }

                   
                    if ((cambiaPaquete || cambiaFechas) &&
                        (reserva.FechaEntrada.Date < paquete.FechaInicio.Date ||
                         reserva.FechaSalida.Date.AddDays(-1) > paquete.FechaFin.Date))
                    {
                        errores.Add((nameof(Reserva.IdPaquete),
                            $"El paquete solo es válido del {paquete.FechaInicio:dd/MM/yyyy} al {paquete.FechaFin:dd/MM/yyyy}."));
                    }
                }
            }

         
            if (errores.Count == 0 && HayConflicto(reserva.IdHabitacion, reserva.FechaEntrada, reserva.FechaSalida, reserva.IdReserva))
            {
                errores.Add((string.Empty, "La habitación no está disponible en esas fechas. Elija otras fechas u otra habitación."));
            }

         
            if (errores.Count == 0 && original != null)
            {
                decimal nuevoMonto = CalcularMontoEdicion(original, reserva, habitacion, paquete);
                decimal pagado = ObtenerTotalPagado(original.IdReserva);

                if (nuevoMonto < pagado)
                {
                    errores.Add((string.Empty,
                        $"El nuevo total ({nuevoMonto.Colones()}) sería menor a lo ya pagado ({pagado.Colones()}). Anule primero los pagos necesarios."));
                }
            }

            return errores;
        }

        
        public int Agregar(Reserva reserva, string idUsuario)
        {
            // Convertir el ID de Identity al ID interno de Usuario (int)
            int idUsuarioInterno = _usuarioService.AsegurarRegistro(idUsuario);

            HabitacionEntidad habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion)
                ?? throw new InvalidOperationException("La habitación no existe.");

            PaqueteEntidad? paquete = reserva.IdPaquete.HasValue
                ? _contexto.Paquetes.Find(reserva.IdPaquete.Value)
                : null;


            using var transaccion = _contexto.Database.BeginTransaction(IsolationLevel.Serializable);

            if (HayConflicto(reserva.IdHabitacion, reserva.FechaEntrada, reserva.FechaSalida))
            {
                transaccion.Rollback();
                return 0;
            }

            ReservaEntidad entidad = new()
            {
                IdUsuario = idUsuarioInterno,
                IdHabitacion = reserva.IdHabitacion,
                IdPaquete = reserva.IdPaquete,
                FechaEntrada = reserva.FechaEntrada.Date,
                FechaSalida = reserva.FechaSalida.Date,
                CantidadPersonas = reserva.CantidadPersonas,
                MontoTotal = CalcularMonto(reserva.FechaEntrada, reserva.FechaSalida, habitacion, paquete),
                Estado = true,
                FechaRegistro = _fechaService.ObtenerFechaActual()
            };

            _contexto.Reservas.Add(entidad);
            _contexto.SaveChanges();
            transaccion.Commit();

            return entidad.IdReserva;
        }

        public bool Editar(Reserva reserva)
        {
            ReservaEntidad? entidad = _contexto.Reservas.Find(reserva.IdReserva);
            HabitacionEntidad? habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion);

            if (entidad == null || habitacion == null || !entidad.Estado)
            {
                return false;
            }

            PaqueteEntidad? paquete = reserva.IdPaquete.HasValue ? _contexto.Paquetes.Find(reserva.IdPaquete.Value) : null;

            using var transaccion = _contexto.Database.BeginTransaction(IsolationLevel.Serializable);

            if (HayConflicto(reserva.IdHabitacion, reserva.FechaEntrada, reserva.FechaSalida, reserva.IdReserva))
            {
                transaccion.Rollback();
                return false;
            }

            entidad.MontoTotal = CalcularMontoEdicion(entidad, reserva, habitacion, paquete);
            entidad.IdHabitacion = reserva.IdHabitacion;
            entidad.IdPaquete = reserva.IdPaquete;
            entidad.FechaEntrada = reserva.FechaEntrada.Date;
            entidad.FechaSalida = reserva.FechaSalida.Date;
            entidad.CantidadPersonas = reserva.CantidadPersonas;

            _contexto.SaveChanges();
            transaccion.Commit();
            return true;
        }

       
        public bool Cancelar(int id)
        {
            ReservaEntidad? entidad = _contexto.Reservas.Find(id);

            if (entidad == null || !entidad.Estado)
            {
                return false;
            }

            entidad.Estado = false;
            return _contexto.SaveChanges() > 0;
        }


        public static decimal CalcularMonto(DateTime entrada, DateTime salida, HabitacionEntidad habitacion, PaqueteEntidad? paquete)
        {
            int noches = (salida.Date - entrada.Date).Days;
            return noches * habitacion.PrecioPorNoche + (paquete?.Precio ?? 0m);
        }

      
        private static decimal CalcularMontoEdicion(ReservaEntidad original, Reserva cambios, HabitacionEntidad habitacion, PaqueteEntidad? paquete)
        {
            bool sinCambios =
                original.IdHabitacion == cambios.IdHabitacion &&
                original.FechaEntrada.Date == cambios.FechaEntrada.Date &&
                original.FechaSalida.Date == cambios.FechaSalida.Date &&
                original.IdPaquete == cambios.IdPaquete;

            return sinCambios
                ? original.MontoTotal
                : CalcularMonto(cambios.FechaEntrada, cambios.FechaSalida, habitacion, paquete);
        }



        private List<Reserva> CompletarInfo(List<ReservaEntidad> entidades)
        {
            List<Reserva> reservas = new List<Reserva>();

            DateTime hoy = _fechaService.ObtenerFechaActual().Date;

            foreach (var e in entidades)
            {
                var habitacion = _contexto.Habitaciones
                    .FirstOrDefault(h => h.IdHabitacion == e.IdHabitacion);

                var paquete = _contexto.Paquetes
                    .FirstOrDefault(p => p.IdPaquete == e.IdPaquete);

                var pagos = _contexto.Pagos
                    .Where(p => p.IdReserva == e.IdReserva && p.Estado == EstadosPago.Aplicado)
                    .Sum(p => (decimal?)p.Monto) ?? 0m;

                var huespedes = _usuarioService.ObtenerNombres(new List<int> { e.IdUsuario });

                string estadoTexto;

                if (!e.Estado)
                {
                    estadoTexto = EstadosReserva.Cancelada;
                }
                else if (e.FechaSalida.Date <= hoy)
                {
                    estadoTexto = EstadosReserva.Finalizada;
                }
                else if (e.FechaEntrada.Date <= hoy)
                {
                    estadoTexto = EstadosReserva.EnCurso;
                }
                else
                {
                    estadoTexto = EstadosReserva.Confirmada;
                }

                Reserva reserva = new Reserva
                {
                    IdReserva = e.IdReserva,
                    IdUsuario = e.IdUsuario,
                    IdHabitacion = e.IdHabitacion,
                    IdPaquete = e.IdPaquete,
                    FechaEntrada = e.FechaEntrada,
                    FechaSalida = e.FechaSalida,
                    CantidadPersonas = e.CantidadPersonas,
                    MontoTotal = e.MontoTotal,
                    Estado = e.Estado,
                    FechaRegistro = e.FechaRegistro,

                    HuespedTexto = huespedes.ContainsKey(e.IdUsuario)
                        ? huespedes[e.IdUsuario]
                        : e.IdUsuario.ToString(),

                    HabitacionTexto = habitacion != null
                        ? habitacion.Codigo + " - " + habitacion.Nombre
                        : e.IdHabitacion.ToString(),

                    PaqueteTexto = paquete != null
                        ? paquete.Nombre
                        : "Sin paquete",

                    TotalPagado = pagos,
                    Saldo = Math.Max(0m, e.MontoTotal - pagos),
                    EstadoTexto = estadoTexto
                };

                reservas.Add(reserva);
            }

            return reservas;
        }

    }
}
