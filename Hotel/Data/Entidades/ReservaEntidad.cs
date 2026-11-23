using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Data.Entidades
{
    [Table("Reserva")]
    public class ReservaEntidad
    {
        [Key]
        public int IdReserva { get; set; }

   
        public string IdUsuario { get; set; } = string.Empty;

        public int IdHabitacion { get; set; }

      
        public int? IdPaquete { get; set; }

        public DateTime FechaEntrada { get; set; }

        public DateTime FechaSalida { get; set; }

        public int CantidadPersonas { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal MontoTotal { get; set; }

   
        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
