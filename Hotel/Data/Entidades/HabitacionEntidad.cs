using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelLosPatitos.Data.Entidades
{
    [Table("HABITACION")]
    public class HabitacionEntidad
    {
        [Key]
        [Column("IdHabitacion")]
        public int IdHabitacion { get; set; }

        [Column("IdTipoHabitacion")]
        public int IdTipoHabitacion { get; set; }

        [Column("Codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("CantidadHuespedes")]
        public int CantidadHuespedes { get; set; }

        [Column("CantidadCamas")]
        public int CantidadCamas { get; set; }

        [Column("CantidadBanos")]
        public int CantidadBanos { get; set; }

        [Column("Ubicacion")]
        public string Ubicacion { get; set; } = string.Empty;

        [Column("PrecioPorNoche", TypeName = "decimal(18,2)")]
        public decimal PrecioPorNoche { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; }

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("FechaModificacion")]
        public DateTime? FechaModificacion { get; set; }
    }
}