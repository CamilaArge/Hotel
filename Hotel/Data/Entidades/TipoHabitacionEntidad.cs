using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Data.Entidades
{
    [Table("TIPOHABITACION")]
    public class TipoHabitacionEntidad
    {
        [Key]
        [Column("IdTipoHabitacion")]
        public int IdTipoHabitacion { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("Estado")]
        public bool Estado { get; set; }

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}