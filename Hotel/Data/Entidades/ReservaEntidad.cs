using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelLosPatitos.Data.Entidades
{
 
    [Table("RESERVACIONES")]
    public class ReservaEntidad
    {
        [Key]
        public int Id { get; set; }

        public int IdUsuario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        public DateTime FechaInicioReserva { get; set; }

        public DateTime FechaFinReserva { get; set; }

        public DateTime FechaDeRegistro { get; set; }

        public int IdHabitacion { get; set; }
    }
}
