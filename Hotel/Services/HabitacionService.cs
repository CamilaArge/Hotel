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
                .OrderBy(h => h.Codigo)
                .ToList()
                .Select(ConvertirAModelo)
                .ToList();
        }

        public List<Habitacion> ObtenerActivas()
        {
            return _contexto.Habitaciones
                .Where(h => h.Estado)
                .OrderBy(h => h.Codigo)
                .ToList()
                .Select(ConvertirAModelo)
                .ToList();
        }

        public Habitacion? ObtenerPorId(int id)
        {
            HabitacionEntidad? entidad = _contexto.Habitaciones.Find(id);
            return entidad == null ? null : ConvertirAModelo(entidad);
        }

      
        public bool ExisteCodigo(string codigo, int idHabitacionExcluir = 0)
        {
            return _contexto.Habitaciones
                .Any(h => h.Codigo == codigo && h.IdHabitacion != idHabitacionExcluir);
        }

        public bool TieneReservas(int idHabitacion)
        {
            return _contexto.Reservas.Any(r => r.IdHabitacion == idHabitacion);
        }

        public bool Agregar(Habitacion habitacion)
        {
            HabitacionEntidad entidad = ConvertirAEntidad(habitacion);
            entidad.IdHabitacion = 0;
            entidad.Estado = true;
            entidad.FechaRegistro = _fechaService.ObtenerFechaActual();
            entidad.FechaModificacion = null;

            _contexto.Habitaciones.Add(entidad);
            return _contexto.SaveChanges() > 0;
        }

        public bool Editar(Habitacion habitacion)
        {
            HabitacionEntidad? entidad = _contexto.Habitaciones.Find(habitacion.IdHabitacion);

            if (entidad == null)
            {
                return false;
            }

            entidad.IdTipoHabitacion = habitacion.IdTipoHabitacion;
            entidad.Descripcion = habitacion.Descripcion;
            entidad.CantidadHuespedes = habitacion.CantidadHuespedes;
            entidad.CantidadCamas = habitacion.CantidadCamas;
            entidad.PrecioPorNoche = habitacion.PrecioPorNoche;
            entidad.Estado = habitacion.Estado;
            entidad.FechaModificacion = _fechaService.ObtenerFechaActual();

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

    
        private static Habitacion ConvertirAModelo(HabitacionEntidad entidad)
        {
            return new Habitacion
            {
                IdHabitacion = entidad.IdHabitacion,
                IdTipoHabitacion = entidad.IdTipoHabitacion,
                Codigo = entidad.Codigo,
                Nombre = entidad.Nombre,
                Descripcion = entidad.Descripcion,
                CantidadHuespedes = entidad.CantidadHuespedes,
                CantidadCamas = entidad.CantidadCamas,
                CantidadBanos = entidad.CantidadBanos,
                Ubicacion = entidad.Ubicacion,
                PrecioPorNoche = entidad.PrecioPorNoche,
                Estado = entidad.Estado,
                FechaRegistro = entidad.FechaRegistro,
                FechaModificacion = entidad.FechaModificacion
            };
        }

        private static HabitacionEntidad ConvertirAEntidad(Habitacion habitacion)
        {
            return new HabitacionEntidad
            {
                IdHabitacion = habitacion.IdHabitacion,
                IdTipoHabitacion = habitacion.IdTipoHabitacion,
                Codigo = habitacion.Codigo,
                Nombre = habitacion.Nombre,
                Descripcion = habitacion.Descripcion,
                CantidadHuespedes = habitacion.CantidadHuespedes,
                CantidadCamas = habitacion.CantidadCamas,
                CantidadBanos = habitacion.CantidadBanos,
                Ubicacion = habitacion.Ubicacion,
                PrecioPorNoche = habitacion.PrecioPorNoche,
                Estado = habitacion.Estado,
                FechaRegistro = habitacion.FechaRegistro,
                FechaModificacion = habitacion.FechaModificacion
            };
        }
    }
}
