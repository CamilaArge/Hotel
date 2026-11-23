using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hotel.Models
{
    public class Reserva
    {
        [Display(Name = "Número de reserva")]
        public int IdReserva { get; set; }

       
        [Display(Name = "Huésped")]
        public string? IdUsuario { get; set; }

        [Display(Name = "Habitación")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una habitación")]
        public int IdHabitacion { get; set; }

        [Display(Name = "Paquete")]
        public int? IdPaquete { get; set; }

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
        [Range(1, 50, ErrorMessage = "Debe ser mayor a 0")]
        public int CantidadPersonas { get; set; } = 1;

        [Display(Name = "Monto total")]
        public decimal MontoTotal { get; set; }

        [Display(Name = "Confirmada")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

      

        [ValidateNever] public string HuespedTexto { get; set; } = string.Empty;
        [ValidateNever] public string HabitacionTexto { get; set; } = string.Empty;
        [ValidateNever] public string PaqueteTexto { get; set; } = "Sin paquete";
        [ValidateNever] public string EstadoTexto { get; set; } = string.Empty;
        [ValidateNever] public decimal TotalPagado { get; set; }
        [ValidateNever] public decimal Saldo { get; set; }

        public int Noches => (FechaSalida.Date - FechaEntrada.Date).Days;

   
        public string EstadoPagoTexto =>
            TotalPagado <= 0 ? "Pendiente" : Saldo <= 0 ? "Completo" : "Parcial";
    }

    public static class EstadosReserva
    {
        public const string Confirmada = "Confirmada";
        public const string EnCurso = "En curso";
        public const string Finalizada = "Finalizada";
        public const string Cancelada = "Cancelada";
    }

  
    public class FiltroReservas
    {
        [Display(Name = "Número de reserva")]
        public int? Numero { get; set; }

        [Display(Name = "Huésped (nombre o correo)")]
        public string? Huesped { get; set; }

        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        public int? IdHabitacion { get; set; }
    }

 
    public class PaqueteOpcion
    {
        public int IdPaquete { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }


    public class HuespedOpcion
    {
        public string Id { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
    }
}
