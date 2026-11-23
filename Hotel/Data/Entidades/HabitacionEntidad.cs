using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Data.Entidades
{
    [Table("Habitacion")]
    public class HabitacionEntidad
    {
        [Key]
        public int IdHabitacion { get; set; }

        public int IdTipoHabitacion { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

  
        public string? Descripcion { get; set; }

        public int CantidadHuespedes { get; set; }

        public int CantidadCamas { get; set; }

        public int CantidadBanos { get; set; }

        public string? Ubicacion { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal PrecioPorNoche { get; set; }

     
        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaModificacion { get; set; }
    }
}
