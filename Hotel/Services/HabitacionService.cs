using HotelLosPatitos.Data;
using HotelLosPatitos.Data.Entidades;
using HotelLosPatitos.Models;

namespace HotelLosPatitos.Services
{
    public class HabitacionService
    {
        private readonly HotelContext _contexto;
        private readonly FechaService _fechaService;

        public HabitacionService(HotelContext contexto, FechaService fechaService)
        {
            _contexto = contexto;
            _fechaService = fechaService;
        }

        public List<Habitacion> ObtenerTodas()
        {
            return _contexto.Habitaciones
                .ToList()
                .Select(ConvertirAModelo)
                .ToList();
        }

        public List<Habitacion> ObtenerActivas()
        {
            return _contexto.Habitaciones
                .Where(h => h.Estado)
                .ToList()
                .Select(ConvertirAModelo)
                .ToList();
        }

        public Habitacion? ObtenerPorId(int id)
        {
            HabitacionEntidad? entidad = _contexto.Habitaciones.Find(id);
            return entidad == null ? null : ConvertirAModelo(entidad);
        }

        public bool Agregar(Habitacion habitacion)
        {
            HabitacionEntidad entidad = ConvertirAEntidad(habitacion);
            entidad.Estado = true;
            entidad.FechaDeRegistro = _fechaService.ObtenerFechaActual();

            _contexto.Habitaciones.Add(entidad);
            return _contexto.SaveChanges() > 0;
        }

        public bool Editar(Habitacion habitacion)
        {
            HabitacionEntidad? entidad = _contexto.Habitaciones.Find(habitacion.Id);

            if (entidad == null)
            {
                return false;
            }

            // Solo se actualizan los campos editables (código, nombre, baños y ubicación no cambian)
            entidad.CantidadDeHuespedesPermitidos = habitacion.CantidadDeHuespedesPermitidos;
            entidad.CantidadDeCamas = habitacion.CantidadDeCamas;
            entidad.EncargadoDeLimpieza = habitacion.EncargadoDeLimpieza;
            entidad.TipoDeHabitacion = habitacion.TipoDeHabitacion;
            entidad.CostoDeLimpieza = habitacion.CostoDeLimpieza;
            entidad.CostoDeReserva = habitacion.CostoDeReserva;
            entidad.Estado = habitacion.Estado;
            entidad.FechaDeModificacion = _fechaService.ObtenerFechaActual();

            return _contexto.SaveChanges() > 0;
        }

        public bool Eliminar(int id)
        {
            HabitacionEntidad? entidad = _contexto.Habitaciones.Find(id);

            if (entidad == null)
            {
                return false;
            }

            _contexto.Habitaciones.Remove(entidad);
            return _contexto.SaveChanges() > 0;
        }

        // ----- Conversiones entre el modelo de la base de datos y el modelo del sistema -----

        private static Habitacion ConvertirAModelo(HabitacionEntidad entidad)
        {
            return new Habitacion
            {
                Id = entidad.Id,
                CodigoDeHabitacion = entidad.CodigoDeHabitacion,
                NombreDeHabitacion = entidad.NombreDeHabitacion,
                CantidadDeHuespedesPermitidos = entidad.CantidadDeHuespedesPermitidos,
                CantidadDeCamas = entidad.CantidadDeCamas,
                CantidadDeBanos = entidad.CantidadDeBanos,
                Ubicacion = entidad.Ubicacion,
                EncargadoDeLimpieza = entidad.EncargadoDeLimpieza,
                TipoDeHabitacion = entidad.TipoDeHabitacion,
                CostoDeLimpieza = entidad.CostoDeLimpieza,
                CostoDeReserva = entidad.CostoDeReserva,
                FechaDeRegistro = entidad.FechaDeRegistro,
                FechaDeModificacion = entidad.FechaDeModificacion,
                Estado = entidad.Estado
            };
        }

        private static HabitacionEntidad ConvertirAEntidad(Habitacion habitacion)
        {
            return new HabitacionEntidad
            {
                Id = habitacion.Id,
                CodigoDeHabitacion = habitacion.CodigoDeHabitacion,
                NombreDeHabitacion = habitacion.NombreDeHabitacion,
                CantidadDeHuespedesPermitidos = habitacion.CantidadDeHuespedesPermitidos,
                CantidadDeCamas = habitacion.CantidadDeCamas,
                CantidadDeBanos = habitacion.CantidadDeBanos,
                Ubicacion = habitacion.Ubicacion,
                EncargadoDeLimpieza = habitacion.EncargadoDeLimpieza,
                TipoDeHabitacion = habitacion.TipoDeHabitacion,
                CostoDeLimpieza = habitacion.CostoDeLimpieza,
                CostoDeReserva = habitacion.CostoDeReserva,
                FechaDeRegistro = habitacion.FechaDeRegistro,
                FechaDeModificacion = habitacion.FechaDeModificacion,
                Estado = habitacion.Estado
            };
        }
    }
}
