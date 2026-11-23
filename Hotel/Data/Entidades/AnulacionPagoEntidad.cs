using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Data.Entidades
{
    [Table("AnulacionPago")]
    public class AnulacionPagoEntidad
    {
        [Key]
        public int IdAnulacion { get; set; }

        public int IdPago { get; set; }


        public int IdUsuario { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public DateTime FechaAnulacion { get; set; }
    }
}
