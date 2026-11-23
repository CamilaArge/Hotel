using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
        [MaxLength(20, ErrorMessage = "Máximo 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Debe ingresar el nombre de la habitación")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "Debe ingresar la descripción")]
        [MaxLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Huéspedes permitidos")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de huéspedes")]
        [Range(1, 50, ErrorMessage = "Debe ser mayor a 0")]
        public int CantidadHuespedes { get; set; }

        [Display(Name = "Camas")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de camas")]
        [Range(0, 50, ErrorMessage = "No puede ser negativo")]
        public int CantidadCamas { get; set; }

        [Display(Name = "Baños")]
        [Required(ErrorMessage = "Debe ingresar la cantidad de baños")]
        [Range(0, 50, ErrorMessage = "No puede ser negativo")]
        public int CantidadBanos { get; set; }

      
        [Display(Name = "Ubicación")]
        [Required(ErrorMessage = "Debe ingresar la ubicación")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
        public string Ubicacion { get; set; } = string.Empty;

        [Display(Name = "Precio por noche")]
        [Required(ErrorMessage = "Debe ingresar el precio por noche")]
        [Range(typeof(decimal), "0.01", "9999999999", ErrorMessage = "Debe ser mayor a 0")]
        public decimal PrecioPorNoche { get; set; }

        [Display(Name = "Activa")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Fecha de modificación")]
        public DateTime? FechaModificacion { get; set; }


        [ValidateNever]
        public string TipoTexto { get; set; } = "Sin tipo";

     
        [ValidateNever]
        [Display(Name = "Estado")]
        public string EstadoOperativo { get; set; } = EstadosHabitacion.Disponible;
    }

    public static class EstadosHabitacion
    {
        public const string Disponible = "Disponible";
        public const string Ocupada = "Ocupada";
        public const string Mantenimiento = "Mantenimiento";
    }
}
