using System.ComponentModel.DataAnnotations;

namespace HotelLosPatitos.Models
{
    // Modelo del sistema: es el que usan los controladores y las vistas (con validaciones)
    public class Reserva
    {
        public int Id { get; set; }

        [Display(Name = "Nombre de la persona")]
        [Required(ErrorMessage = "Debe ingresar el nombre de la persona")]
        [MaxLength(150, ErrorMessage = "Máximo 150 caracteres")]
        public string NombrePersona { get; set; } = string.Empty;

        [Display(Name = "Identificación")]
        [Required(ErrorMessage = "Debe ingresar la identificación")]
        [MaxLength(30, ErrorMessage = "Máximo 30 caracteres")]
        public string Identificacion { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [Required(ErrorMessage = "Debe ingresar el teléfono")]
        [MaxLength(10, ErrorMessage = "Máximo 10 caracteres")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Correo")]
        [Required(ErrorMessage = "Debe ingresar el correo")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido")]
        [MaxLength(50, ErrorMessage = "Máximo 50 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Fecha de nacimiento")]
        [Required(ErrorMessage = "Debe ingresar la fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "Debe ingresar la dirección")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
        public string Direccion { get; set; } = string.Empty;

        // Lo calcula ReservaService, por eso no se pide en el formulario
        [Display(Name = "Monto total")]
        public decimal MontoTotal { get; set; }

        [Display(Name = "Fecha de entrada")]
        [Required(ErrorMessage = "Debe ingresar la fecha de inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicioReserva { get; set; } = DateTime.Today;

        [Display(Name = "Fecha de salida")]
        [Required(ErrorMessage = "Debe ingresar la fecha de fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFinReserva { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Fecha de registro")]
        public DateTime FechaDeRegistro { get; set; }

        [Display(Name = "Habitación")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una habitación válida")]
        public int IdHabitacion { get; set; }
    }
}
