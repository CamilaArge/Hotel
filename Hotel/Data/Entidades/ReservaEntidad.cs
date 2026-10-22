using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelLosPatitos.Data.Entidades
{
    // Modelo de la base de datos: representa la tabla RESERVACIONES tal como está
    [Table("RESERVACIONES")]
    public class ReservaEntidad
    {
        [Key]
        public int Id { get; set; }

        [Column("NombreDeLaPersona")]
        public string NombrePersona { get; set; } = string.Empty;

        public string Identificacion { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        public string Direccion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        public DateTime FechaInicioReserva { get; set; }

        public DateTime FechaFinReserva { get; set; }

        public DateTime FechaDeRegistro { get; set; }

        public int IdHabitacion { get; set; }
    }
}
