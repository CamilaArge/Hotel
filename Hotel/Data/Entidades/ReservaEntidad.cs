using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Data.Entidades
{
    [Table("RESERVA")]
    public class ReservaEntidad
    {
        [Key]
        [Column("IdReserva")]
        public int IdReserva { get; set; }

        [Column("IdUsuario")]
        public string IdUsuario { get; set; }

        [Column("IdHabitacion")]
        public int IdHabitacion { get; set; }

        [Column("IdPaquete")]
        public int IdPaquete { get; set; }

        [Column("FechaEntrada")]
        public DateTime FechaEntrada { get; set; }

        [Column("FechaSalida")]
        public DateTime FechaSalida { get; set; }

        [Column("CantidadPersonas")]
        public int CantidadPersonas { get; set; }

        [Column("MontoTotal", TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; }

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}