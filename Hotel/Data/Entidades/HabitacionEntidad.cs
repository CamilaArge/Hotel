using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelLosPatitos.Data.Entidades
{
    // Modelo de la base de datos: representa la tabla HABITACIONES tal como está
    [Table("HABITACIONES")]
    public class HabitacionEntidad
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        public string CodigoDeHabitacion { get; set; } = string.Empty;

        public string NombreDeHabitacion { get; set; } = string.Empty;

        public int CantidadDeHuespedesPermitidos { get; set; }

        public int CantidadDeCamas { get; set; }

        public int CantidadDeBanos { get; set; }

        public string Ubicacion { get; set; } = string.Empty;

        public string EncargadoDeLimpieza { get; set; } = string.Empty;

        public int TipoDeHabitacion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoDeLimpieza { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoDeReserva { get; set; }

        public DateTime FechaDeRegistro { get; set; }

        public DateTime? FechaDeModificacion { get; set; }

        public bool Estado { get; set; }
    }
}
