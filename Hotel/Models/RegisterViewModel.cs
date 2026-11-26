using System.ComponentModel.DataAnnotations;

namespace Hotel.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "Nombre completo")]
        [Required(ErrorMessage = "Debe ingresar su nombre")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Cédula")]
        [Required(ErrorMessage = "Debe ingresar su cédula")]
        [StringLength(20, ErrorMessage = "La cédula no puede superar los 20 caracteres")]
        public string Cedula { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Debe ingresar el correo")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [Required(ErrorMessage = "Debe ingresar su teléfono")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Dirección")]
        [StringLength(250, ErrorMessage = "La dirección no puede superar los 250 caracteres")]
        public string? Direccion { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "Debe ingresar la contraseña")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "La contraseña debe tener al menos {2} caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirmar contraseña")]
        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}