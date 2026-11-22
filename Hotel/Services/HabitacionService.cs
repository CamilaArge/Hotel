using Hotel.Data;
using Hotel.Data.Entidades;
using Hotel.Models;

namespace Hotel.Services
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
            Dictionary<int, string> tipos = ObtenerNombresTipos();

            return _contexto.Habitaciones
                .OrderBy(h => h.Codigo)
                .ToList()
                .Select(h => ConvertirAModelo(h, tipos))
                .ToList();
        }

        public List<Habitacion> ObtenerActivas()
        {
            Dictionary<int, string> tipos = ObtenerNombresTipos();

            return _contexto.Habitaciones
                .Where(h => h.Estado)
                .OrderBy(h => h.Codigo)
                .ToList()
                .Select(h => ConvertirAModelo(h, tipos))
                .ToList();
        }

        public Habitacion? ObtenerPorId(int id)
        {
            HabitacionEntidad? entidad = _contexto.Habitaciones.Find(id);
            return entidad == null ? null : ConvertirAModelo(entidad, ObtenerNombresTipos());
        }

        
        public bool ExisteCodigo(string codigo, int idExcluir = 0)
        {
            string codigoLimpio = codigo.Trim().ToUpper();

            return _contexto.Habitaciones
                .Any(h => h.Codigo.ToUpper() == codigoLimpio && h.IdHabitacion != idExcluir);
        }

       
        public bool TieneReservas(int idHabitacion)
        {
            return _contexto.Reservas.Any(r => r.IdHabitacion == idHabitacion);
        }


        public bool TieneReservasVigentes(int idHabitacion)
        {
            DateTime hoy = _fechaService.ObtenerFechaActual().Date;

            return _contexto.Reservas.Any(r =>
                r.IdHabitacion == idHabitacion &&
                r.Estado &&
                r.FechaSalida >= hoy);
        }

        public bool Agregar(Habitacion habitacion)
        {
            HabitacionEntidad entidad = ConvertirAEntidad(habitacion);
            entidad.Codigo = entidad.Codigo.Trim();
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

            // Solo se actualizan los campos editables (código, nombre, baños y ubicación no cambian)
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

        // Nombres de los tipos desde la tabla TipoHabitacion (son pocos, se leen de una vez)
        private Dictionary<int, string> ObtenerNombresTipos()
        {
            return _contexto.TiposHabitacion
                .ToDictionary(t => t.IdTipoHabitacion, t => t.Nombre);
        }

        // ----- Conversiones entre la entidad de la base de datos y el modelo del sistema -----

        private static Habitacion ConvertirAModelo(HabitacionEntidad entidad, Dictionary<int, string> tipos)
        {
            return new Habitacion
            {
                IdHabitacion = entidad.IdHabitacion,
                IdTipoHabitacion = entidad.IdTipoHabitacion,
                TipoTexto = tipos.TryGetValue(entidad.IdTipoHabitacion, out string? nombreTipo) ? nombreTipo : "Sin tipo",
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