using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hotel.Models
{
    public static class MetodosPago
    {
        public const string Efectivo = "Efectivo";
        public const string Tarjeta = "Tarjeta";
        public const string Transferencia = "Transferencia";

        public static readonly string[] Todos = { Efectivo, Tarjeta, Transferencia };
    }

    public static class EstadosPago
    {
        public const string Aplicado = "Aplicado";
        public const string Anulado = "Anulado";

        public static readonly string[] Todos = { Aplicado, Anulado };
    }

  
    public class Pago
    {
        public int IdPago { get; set; }
        public int IdReserva { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string RegistradoPor { get; set; } = string.Empty;

 
        public string? AnuladoPor { get; set; }
        public string? MotivoAnulacion { get; set; }
        public DateTime? FechaAnulacion { get; set; }

        public bool EstaAnulado => Estado == EstadosPago.Anulado;
    }

 
    public class RegistrarPagoViewModel
    {
        public int IdReserva { get; set; }

        [Display(Name = "Monto")]
        [Required(ErrorMessage = "Debe ingresar el monto")]
        [Range(typeof(decimal), "0.01", "9999999999", ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal? Monto { get; set; }

        [Display(Name = "Fecha de pago")]
        [Required(ErrorMessage = "Debe ingresar la fecha del pago")]
        [DataType(DataType.Date)]
        public DateTime? FechaPago { get; set; }

        [Display(Name = "Método de pago")]
        [Required(ErrorMessage = "Debe seleccionar el método de pago")]
        public string? MetodoPago { get; set; }
    }

    public class PagosReservaViewModel
    {
        public Reserva Reserva { get; set; } = new();
        public List<Pago> Pagos { get; set; } = new();
        public RegistrarPagoViewModel NuevoPago { get; set; } = new();
    }

 
    public class AnularPagoViewModel
    {
        public int IdPago { get; set; }

        [Display(Name = "Motivo de la anulación")]
        [Required(ErrorMessage = "Debe indicar el motivo de la anulación")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre 5 y 500 caracteres")]
        public string Motivo { get; set; } = string.Empty;

        [ValidateNever]
        public Pago? Pago { get; set; }
    }

   
    public class HistorialPagosViewModel
    {
        [Display(Name = "Número de reserva")]
        public int? IdReserva { get; set; }

        [Display(Name = "Desde")]
        [DataType(DataType.Date)]
        public DateTime? Desde { get; set; }

        [Display(Name = "Hasta")]
        [DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }

        [Display(Name = "Método")]
        public string? Metodo { get; set; }

        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        [ValidateNever]
        public List<Pago> Resultados { get; set; } = new();

        public decimal TotalAplicado => Resultados.Where(p => !p.EstaAnulado).Sum(p => p.Monto);
    }
}
