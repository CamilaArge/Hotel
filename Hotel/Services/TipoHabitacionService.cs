using Hotel.Models;
using Hotel.Data;

namespace Hotel.Services
{
    public class TipoHabitacionService
    {
        private readonly HotelContext _contexto;

        public TipoHabitacionService(HotelContext contexto)
        {
            _contexto = contexto;
        }

       
        public List<TipoHabitacion> ObtenerActivos()
        {
            return _contexto.TiposHabitacion
                .Where(t => t.Estado)
                .OrderBy(t => t.Nombre)
                .Select(t => new TipoHabitacion
                {
                    IdTipoHabitacion = t.IdTipoHabitacion,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion ?? string.Empty,
                    Estado = t.Estado,
                    FechaRegistro = t.FechaRegistro
                })
                .ToList();
        }

     
        public List<TipoHabitacion> ObtenerParaSelect(int idSeleccionado)
        {
            return _contexto.TiposHabitacion
                .Where(t => t.Estado || t.IdTipoHabitacion == idSeleccionado)
                .OrderBy(t => t.Nombre)
                .Select(t => new TipoHabitacion
                {
                    IdTipoHabitacion = t.IdTipoHabitacion,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion ?? string.Empty,
                    Estado = t.Estado,
                    FechaRegistro = t.FechaRegistro
                })
                .ToList();
        }

        public bool ExisteActivo(int idTipoHabitacion)
        {
            return _contexto.TiposHabitacion
                .Any(t => t.IdTipoHabitacion == idTipoHabitacion && t.Estado);
        }
    }
}