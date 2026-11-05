using HotelLosPatitos.Data;
using HotelLosPatitos.Data.Entidades;
using HotelLosPatitos.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelLosPatitos.Services
{
    public class ReservaService
    {
        private readonly HotelContext _contexto;
        private readonly FechaService _fechaService;

        public ReservaService(HotelContext contexto, FechaService fechaService)
        {
            _contexto = contexto;
            _fechaService = fechaService;
        }

        public List<Reserva> ObtenerPorHabitacion(int idHabitacion)
        {
            return _contexto.Reservas
                .Where(r => r.IdHabitacion == idHabitacion)
                .OrderByDescending(r => r.FechaEntrada)
                .ToList()
                .Select(ConvertirAModelo)
                .ToList();
        }

        public Reserva? ObtenerPorId(int id)
        {
            ReservaEntidad? entidad = _contexto.Reservas.Find(id);
            return entidad == null ? null : ConvertirAModelo(entidad);
        }



      
        public bool HayChoqueDeFechas(Reserva reserva)
        {
            return _contexto.Reservas.Any(r =>
                r.IdHabitacion == reserva.IdHabitacion &&
                r.IdReserva != reserva.IdReserva &&
                r.Estado &&
                reserva.FechaEntrada.Date < r.FechaSalida &&
                reserva.FechaSalida.Date > r.FechaEntrada);
        }

      
        public int Agregar(Reserva reserva)
        {
            HabitacionEntidad? habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion);

            if (habitacion == null)
            {
                return 0;
            }

            ReservaEntidad entidad = ConvertirAEntidad(reserva);
            entidad.IdReserva = 0;
            entidad.Estado = true;
            entidad.FechaRegistro = _fechaService.ObtenerFechaActual();
            entidad.MontoTotal = CalcularMonto(reserva, habitacion);

            _contexto.Reservas.Add(entidad);
            _contexto.SaveChanges();

            return entidad.IdReserva;
        }

        public bool Editar(Reserva reserva)
        {
            ReservaEntidad? entidad = _contexto.Reservas.Find(reserva.IdReserva);
            HabitacionEntidad? habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion);

            if (entidad == null || habitacion == null)
            {
                return false;
            }

            // El usuario, el paquete y la fecha de registro no se modifican
            entidad.IdHabitacion = reserva.IdHabitacion;
            entidad.FechaEntrada = reserva.FechaEntrada.Date;
            entidad.FechaSalida = reserva.FechaSalida.Date;
            entidad.CantidadPersonas = reserva.CantidadPersonas;
            entidad.Estado = reserva.Estado;
            entidad.MontoTotal = CalcularMonto(reserva, habitacion);

            return _contexto.SaveChanges() > 0;
        }

        public bool Eliminar(int id)
        {
            ReservaEntidad? entidad = _contexto.Reservas.Find(id);

            if (entidad == null)
            {
                return false;
            }

            _contexto.Reservas.Remove(entidad);
            return _contexto.SaveChanges() > 0;
        }

   
        private static decimal CalcularMonto(Reserva reserva, HabitacionEntidad habitacion)
        {
            int noches = (reserva.FechaSalida.Date - reserva.FechaEntrada.Date).Days;
            return noches * habitacion.PrecioPorNoche;
        }

   
        private static Reserva ConvertirAModelo(ReservaEntidad entidad)
        {
            return new Reserva
            {
                IdReserva = entidad.IdReserva,
                IdUsuario = entidad.IdUsuario,
                IdHabitacion = entidad.IdHabitacion,
                IdPaquete = entidad.IdPaquete,
                FechaEntrada = entidad.FechaEntrada,
                FechaSalida = entidad.FechaSalida,
                CantidadPersonas = entidad.CantidadPersonas,
                MontoTotal = entidad.MontoTotal,
                Estado = entidad.Estado,
                FechaRegistro = entidad.FechaRegistro
            };
        }

        private static ReservaEntidad ConvertirAEntidad(Reserva reserva)
        {
            return new ReservaEntidad
            {
                IdReserva = reserva.IdReserva,
                IdUsuario = reserva.IdUsuario,
                IdHabitacion = reserva.IdHabitacion,
                IdPaquete = reserva.IdPaquete,
                FechaEntrada = reserva.FechaEntrada.Date,
                FechaSalida = reserva.FechaSalida.Date,
                CantidadPersonas = reserva.CantidadPersonas,
                MontoTotal = reserva.MontoTotal,
                Estado = reserva.Estado,
                FechaRegistro = reserva.FechaRegistro
            };
        }
    }
}
