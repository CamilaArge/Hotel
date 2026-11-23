using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Data.Entidades
{
    [Table("Pago")]
    public class PagoEntidad
    {
        [Key]
        public int IdPago { get; set; }

        public int IdReserva { get; set; }


        public int IdUsuarioRegistro { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

     
        public string Estado { get; set; } = string.Empty;
    }
}
