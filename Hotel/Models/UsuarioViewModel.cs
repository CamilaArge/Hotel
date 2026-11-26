using System.ComponentModel.DataAnnotations;

namespace Hotel.Models
{
    public class UsuarioViewModel
    {
        [Display(Name = "Nombre completo")]
        [Required(ErrorMessage = "Debe ingresar el nombre")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Cédula")]
        [Required(ErrorMessage = "Debe ingresar la cédula")]
        [StringLength(20)]
        public string Cedula { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Debe ingresar el correo")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [Required(ErrorMessage = "Debe ingresar el teléfono")]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Dirección")]
        [StringLength(250)]
        public string? Direccion { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirmar contraseña")]
        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public string Rol { get; set; } = Roles.Cliente;
    }
}