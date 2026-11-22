using System.ComponentModel.DataAnnotations;

namespace Hotel.Models
{
    public class Habitacion
    {
        public int IdHabitacion { get; set; }

        [Display(Name = "Tipo de habitación")]
        [Required(ErrorMessage = "Debe seleccionar el tipo de habitación")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el tipo de habitación")]
        public int IdTipoHabitacion { get; set; }

        [Display(Name = "Código")]
        [Required(ErrorMessage = "Debe ingresar el código de la habitación")]
        [MaxLength(7, ErrorMessage = "Máximo 7 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Debe ingresar el nombre de la habitación")]
        [MaxLength(30, ErrorMessage = "Máximo 30 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "Debe ingresar la descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Huéspedes permitidos")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de huéspedes")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe ser mayor a 0")]
        public int CantidadHuespedes { get; set; }

        [Display(Name = "Camas")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de camas")]
        [Range(0, int.MaxValue, ErrorMessage = "No puede ser negativo")]
        public int CantidadCamas { get; set; }

        [Display(Name = "Baños")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de baños")]
        [Range(0, int.MaxValue, ErrorMessage = "No puede ser negativo")]
        public int CantidadBanos { get; set; }

        [Display(Name = "Ubicación")]
        [Required(ErrorMessage = "Debe ingresar la ubicación")]
        [MaxLength(10, ErrorMessage = "Máximo 10 caracteres")]
        public string Ubicacion { get; set; } = string.Empty;

        [Display(Name = "Precio por noche")]
        [Required(ErrorMessage = "Debe ingresar el precio por noche")]
        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Debe ser mayor a 0")]
        public decimal PrecioPorNoche { get; set; }

        [Display(Name = "Activa")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Fecha de modificación")]
        public DateTime? FechaModificacion { get; set; }

        public string TipoTexto { get; set; } = "Sin tipo";
    }
}