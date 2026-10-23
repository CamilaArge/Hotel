using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelLosPatitos.Data.Entidades
{
   
    [Table("HABITACIONES")]
    public class HabitacionEntidad
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public string CodigoDeHabitacion { get; set; } = string.Empty;

        public string NombreHabitacion { get; set; } = string.Empty;

        public int CantidadHuespedesPermitidos { get; set; }

        public int CantidadCamas { get; set; }

        public int CantidadBanos { get; set; }

        public string Ubicacion { get; set; } = string.Empty;

        public int TipoDeHabitacion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoDeReserva { get; set; }

        public DateTime FechaDeRegistro { get; set; }

        public DateTime? FechaDeModificacion { get; set; }

        public bool Estado { get; set; }
    }
}
