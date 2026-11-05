using System.ComponentModel.DataAnnotations;

namespace HotelLosPatitos.Models
{
    public class Reserva
    {
        public int IdReserva { get; set; }

        [Display(Name = "Usuario")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un usuario válido")]
        public int IdUsuario { get; set; }

        [Display(Name = "Habitación")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una habitación válida")]
        public int IdHabitacion { get; set; }

        [Display(Name = "Paquete")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un paquete válido")]
        public int IdPaquete { get; set; }

        [Display(Name = "Fecha de entrada")]
        [Required(ErrorMessage = "Debe ingresar la fecha de entrada")]
        [DataType(DataType.Date)]
        public DateTime FechaEntrada { get; set; } = DateTime.Today;

        [Display(Name = "Fecha de salida")]
        [Required(ErrorMessage = "Debe ingresar la fecha de salida")]
        [DataType(DataType.Date)]
        public DateTime FechaSalida { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Cantidad de personas")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de personas")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe ser mayor a 0")]
        public int CantidadPersonas { get; set; }

        [Display(Name = "Monto total")]
        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoTotal { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }
    }
}