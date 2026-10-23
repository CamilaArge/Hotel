using HotelLosPatitos.Data;
using HotelLosPatitos.Data.Entidades;
using HotelLosPatitos.Models;

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
                .ToList()
                .Select(ConvertirAModelo)
                .ToList();
        }

        public Reserva? ObtenerPorId(int id)
        {
            ReservaEntidad? entidad = _contexto.Reservas.Find(id);
            return entidad == null ? null : ConvertirAModelo(entidad);
        }

        // Devuelve el Id de la reserva creada (0 si no se pudo guardar)
        public int Agregar(Reserva reserva)
        {
            HabitacionEntidad? habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion);

            if (habitacion == null)
            {
                return 0;
            }

            ReservaEntidad entidad = ConvertirAEntidad(reserva);
            entidad.FechaDeRegistro = _fechaService.ObtenerFechaActual();
            entidad.MontoTotal = CalcularMonto(reserva, habitacion);

            _contexto.Reservas.Add(entidad);
            _contexto.SaveChanges();

            return entidad.Id;
        }

        public bool Editar(Reserva reserva)
        {
            ReservaEntidad? entidad = _contexto.Reservas.Find(reserva.Id);
            HabitacionEntidad? habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion);

            if (entidad == null || habitacion == null)
            {
                return false;
            }

            entidad.FechaInicioReserva = reserva.FechaInicioReserva;
            entidad.FechaFinReserva = reserva.FechaFinReserva;
            entidad.IdHabitacion = reserva.IdHabitacion;
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

        // Monto = noches * costo por noche + costo de limpieza
        private static decimal CalcularMonto(Reserva reserva, HabitacionEntidad habitacion)
        {
            int noches = (reserva.FechaFinReserva.Date - reserva.FechaInicioReserva.Date).Days;
            return (noches * habitacion.CostoDeReserva);
        }

        // ----- Conversiones entre el modelo de la base de datos y el modelo del sistema -----

        private static Reserva ConvertirAModelo(ReservaEntidad entidad)
        {
            return new Reserva
            {
                Id = entidad.Id,
                MontoTotal = entidad.MontoTotal,
                FechaInicioReserva = entidad.FechaInicioReserva,
                FechaFinReserva = entidad.FechaFinReserva,
                FechaDeRegistro = entidad.FechaDeRegistro,
                IdHabitacion = entidad.IdHabitacion
            };
        }

        private static ReservaEntidad ConvertirAEntidad(Reserva reserva)
        {
            return new ReservaEntidad
            {
                Id = reserva.Id,
                MontoTotal = reserva.MontoTotal,
                FechaInicioReserva = reserva.FechaInicioReserva,
                FechaFinReserva = reserva.FechaFinReserva,
                FechaDeRegistro = reserva.FechaDeRegistro,
                IdHabitacion = reserva.IdHabitacion
            };
        }
    }
}
