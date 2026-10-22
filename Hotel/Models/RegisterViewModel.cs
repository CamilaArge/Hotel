using System.ComponentModel.DataAnnotations;

namespace HotelLosPatitos.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Debe ingresar el correo")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "Debe ingresar la contraseña")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos {2} caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirmar contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public string Rol { get; set; } = string.Empty;
    }
}
