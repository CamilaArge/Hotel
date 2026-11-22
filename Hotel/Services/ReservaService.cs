using Hotel.Data;
using Hotel.Data.Entidades;
using Hotel.Models;

namespace Hotel.Services
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

        // HU-03 CA-02 / RESERVAS-CP-003: la habitación no puede tener otra reserva activa que se cruce.
        // Una salida el mismo día de otra entrada no es conflicto. Al editar se excluye la propia reserva.
        public bool HayConflicto(int idHabitacion, DateTime entrada, DateTime salida, int idReservaExcluir = 0)
        {
            return _contexto.Reservas.Any(r =>
                r.IdHabitacion == idHabitacion &&
                r.IdReserva != idReservaExcluir &&
                r.Estado &&
                r.FechaEntrada < salida.Date &&
                r.FechaSalida > entrada.Date);
        }

        // Valida las reglas de negocio de una reserva y devuelve los errores como (campo, mensaje).
        // Una lista vacía significa que la reserva es válida.
        public List<(string Campo, string Mensaje)> Validar(Reserva reserva, bool esNueva)
        {
            List<(string Campo, string Mensaje)> errores = new();

            HabitacionEntidad? habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion);

            if (habitacion == null)
            {
                errores.Add((nameof(Reserva.IdHabitacion), "La habitación seleccionada no existe."));
                return errores;
            }

            if (!habitacion.Estado)
            {
                errores.Add((nameof(Reserva.IdHabitacion), "La habitación no está activa para nuevas reservas."));
            }

            // RESERVAS-CP-002: la salida debe ser posterior a la entrada
            if (reserva.FechaSalida.Date <= reserva.FechaEntrada.Date)
            {
                errores.Add((nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior a la fecha de entrada."));
                return errores;
            }

            if (esNueva && reserva.FechaEntrada.Date < _fechaService.ObtenerFechaActual().Date)
            {
                errores.Add((nameof(Reserva.FechaEntrada), "La fecha de entrada no puede ser anterior a hoy."));
            }

            // RESERVAS-CP-010: no se puede superar la capacidad de la habitación
            if (reserva.CantidadPersonas > habitacion.CantidadHuespedes)
            {
                errores.Add((nameof(Reserva.CantidadPersonas),
                    $"La habitación permite como máximo {habitacion.CantidadHuespedes} huésped(es)."));
            }

            // RESERVAS-CP-003 y RESERVAS-CP-009: disponibilidad
            if (errores.Count == 0 && HayConflicto(reserva.IdHabitacion, reserva.FechaEntrada, reserva.FechaSalida, reserva.IdReserva))
            {
                errores.Add((string.Empty, "La habitación no está disponible en esas fechas. Elija otras fechas u otra habitación."));
            }

            return errores;
        }

        // Devuelve el Id de la reserva creada (0 si no se pudo guardar)
        public int Agregar(Reserva reserva, string? idUsuario)
        {
            HabitacionEntidad? habitacion = _contexto.Habitaciones.Find(reserva.IdHabitacion);

            if (habitacion == null)
            {
                return 0;
            }

            ReservaEntidad entidad = ConvertirAEntidad(reserva);
            entidad.IdReserva = 0;
            entidad.IdUsuario = idUsuario;
            entidad.FechaEntrada = reserva.FechaEntrada.Date;
            entidad.FechaSalida = reserva.FechaSalida.Date;
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

            if (entidad == null || habitacion == null || !entidad.Estado)
            {
                return false;
            }

            entidad.IdHabitacion = reserva.IdHabitacion;
            entidad.FechaEntrada = reserva.FechaEntrada.Date;
            entidad.FechaSalida = reserva.FechaSalida.Date;
            entidad.CantidadPersonas = reserva.CantidadPersonas;
            entidad.MontoTotal = CalcularMonto(reserva, habitacion);

            return _contexto.SaveChanges() > 0;
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
                FechaEntrada = reserva.FechaEntrada,
                FechaSalida = reserva.FechaSalida,
                CantidadPersonas = reserva.CantidadPersonas,
                MontoTotal = reserva.MontoTotal,
                Estado = reserva.Estado,
                FechaRegistro = reserva.FechaRegistro
            };
        }
    }
}
