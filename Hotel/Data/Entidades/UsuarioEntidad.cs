using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Data.Entidades
{
 
    [Table("Usuario")]
    public class UsuarioEntidad
    {
        [Key]
        public int IdUsuario { get; set; }

        public string? IdentityId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Cedula { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string Correo { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        public string ContrasenaHash { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
