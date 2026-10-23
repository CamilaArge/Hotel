using System.ComponentModel.DataAnnotations;

namespace HotelLosPatitos.Models
{
   
    public class Habitacion
    {
        public int Id { get; set; }

        [Display(Name = "Código")]
        [Required(ErrorMessage = "Debe ingresar el código de la habitación")]
        [MaxLength(7, ErrorMessage = "Máximo 7 caracteres")]
        public string CodigoDeHabitacion { get; set; } = string.Empty;

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Debe ingresar el nombre de la habitación")]
        [MaxLength(30, ErrorMessage = "Máximo 30 caracteres")]
        public string NombreDeHabitacion { get; set; } = string.Empty;

        [Display(Name = "Huéspedes permitidos")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de huéspedes")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe ser mayor a 0")]
        public int CantidadHuespedesPermitidos { get; set; }

        [Display(Name = "Camas")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de camas")]
        [Range(0, int.MaxValue, ErrorMessage = "No puede ser negativo")]
        public int CantidadCamas { get; set; }

        [Display(Name = "Baños")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de baños")]
        [Range(0, int.MaxValue, ErrorMessage = "No puede ser negativo")]
        public int CantidadDeBanos { get; set; }

        [Display(Name = "Ubicación")]
        [Required(ErrorMessage = "Debe ingresar la ubicación")]
        [MaxLength(10, ErrorMessage = "Máximo 10 caracteres")]
        public string Ubicacion { get; set; } = string.Empty;

        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "Debe seleccionar el tipo de habitación")]
        [Range(1, 3, ErrorMessage = "Debe seleccionar el tipo de habitación")]
        public int TipoDeHabitacion { get; set; }

        [Display(Name = "Costo por noche")]
        [Required(ErrorMessage = "Debe ingresar el costo de reserva")]
        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Debe ser mayor a 0")]
        public decimal CostoDeReserva { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaDeRegistro { get; set; }

        [Display(Name = "Fecha de modificación")]
        public DateTime? FechaDeModificacion { get; set; }

        [Display(Name = "Activa")]
        public bool Estado { get; set; } = true;

       
        public string TipoTexto => TipoDeHabitacion switch
        {
            1 => "Junior",
            2 => "Superior",
            3 => "Suite",
            _ => "Sin tipo"
        };
    }
}
