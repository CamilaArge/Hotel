using System.ComponentModel.DataAnnotations;

namespace HotelLosPatitos.Models
{
   
    public class Reserva
    {
        public int Id { get; set; }

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
