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
            return Convertir(_contexto.Habitaciones.OrderBy(h => h.Codigo).ToList());
        }

        public List<Habitacion> ObtenerActivas()
        {
            return Convertir(_contexto.Habitaciones.Where(h => h.Estado).OrderBy(h => h.Codigo).ToList());
        }

  
        public List<Habitacion> ObtenerDisponibles(DateTime entrada, DateTime salida, int personas)
        {
            DateTime e = entrada.Date;
            DateTime s = salida.Date;

            IQueryable<int> ocupadas = _contexto.Reservas
                .Where(r => r.Estado && r.FechaEntrada < s && r.FechaSalida > e)
                .Select(r => r.IdHabitacion);

            List<HabitacionEntidad> libres = _contexto.Habitaciones
                .Where(h => h.Estado && h.CantidadHuespedes >= personas && !ocupadas.Contains(h.IdHabitacion))
                .OrderBy(h => h.PrecioPorNoche)
                .ToList();

            return Convertir(libres);
        }

        public Habitacion? ObtenerPorId(int id)
        {
            HabitacionEntidad? entidad = _contexto.Habitaciones.Find(id);
            return entidad == null ? null : Convertir(new List<HabitacionEntidad> { entidad })[0];
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
                r.FechaSalida > hoy);
        }

        public bool Agregar(Habitacion habitacion)
        {
            HabitacionEntidad entidad = new()
            {
                IdTipoHabitacion = habitacion.IdTipoHabitacion,
                Codigo = habitacion.Codigo.Trim().ToUpper(),
                Nombre = habitacion.Nombre.Trim(),
                Descripcion = habitacion.Descripcion?.Trim(),
                CantidadHuespedes = habitacion.CantidadHuespedes,
                CantidadCamas = habitacion.CantidadCamas,
                CantidadBanos = habitacion.CantidadBanos,
                Ubicacion = habitacion.Ubicacion?.Trim(),
                PrecioPorNoche = habitacion.PrecioPorNoche,
                Estado = true,
                FechaRegistro = _fechaService.ObtenerFechaActual(),
                FechaModificacion = null
            };

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
            entidad.Nombre = habitacion.Nombre.Trim();
            entidad.Descripcion = habitacion.Descripcion?.Trim();
            entidad.CantidadHuespedes = habitacion.CantidadHuespedes;
            entidad.CantidadCamas = habitacion.CantidadCamas;
            entidad.CantidadBanos = habitacion.CantidadBanos;
            entidad.Ubicacion = habitacion.Ubicacion?.Trim();
            entidad.PrecioPorNoche = habitacion.PrecioPorNoche;
            entidad.Estado = habitacion.Estado;
            entidad.FechaModificacion = _fechaService.ObtenerFechaActual();

            return _contexto.SaveChanges() > 0;
        }

     
        public bool CambiarEstado(int idHabitacion, bool activa)
        {
            HabitacionEntidad? entidad = _contexto.Habitaciones.Find(idHabitacion);

            if (entidad == null)
            {
                return false;
            }

            entidad.Estado = activa;
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


        private List<Habitacion> Convertir(List<HabitacionEntidad> entidades)
        {
            Dictionary<int, string> tipos = _contexto.TiposHabitacion
                .ToDictionary(t => t.IdTipoHabitacion, t => t.Nombre);

          
            DateTime hoy = _fechaService.ObtenerFechaActual().Date;
            HashSet<int> ocupadasHoy = _contexto.Reservas
                .Where(r => r.Estado && r.FechaEntrada <= hoy && r.FechaSalida > hoy)
                .Select(r => r.IdHabitacion)
                .Distinct()
                .ToHashSet();

            return entidades.Select(entidad => new Habitacion
            {
                IdHabitacion = entidad.IdHabitacion,
                IdTipoHabitacion = entidad.IdTipoHabitacion,
                TipoTexto = tipos.TryGetValue(entidad.IdTipoHabitacion, out string? nombreTipo) ? nombreTipo : "Sin tipo",
                Codigo = entidad.Codigo,
                Nombre = entidad.Nombre,
                Descripcion = entidad.Descripcion ?? string.Empty,
                CantidadHuespedes = entidad.CantidadHuespedes,
                CantidadCamas = entidad.CantidadCamas,
                CantidadBanos = entidad.CantidadBanos,
                Ubicacion = entidad.Ubicacion ?? string.Empty,
                PrecioPorNoche = entidad.PrecioPorNoche,
                Estado = entidad.Estado,
                FechaRegistro = entidad.FechaRegistro,
                FechaModificacion = entidad.FechaModificacion,
                EstadoOperativo = !entidad.Estado
                    ? EstadosHabitacion.Mantenimiento
                    : ocupadasHoy.Contains(entidad.IdHabitacion)
                        ? EstadosHabitacion.Ocupada
                        : EstadosHabitacion.Disponible
            }).ToList();
        }
    }
}
