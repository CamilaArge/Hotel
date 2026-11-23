using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Data.Entidades
{
 
    [Table("Paquete")]
    public class PaqueteEntidad
    {
        [Key]
        public int IdPaquete { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Precio { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; }
    }
}
