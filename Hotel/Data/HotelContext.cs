using Hotel.Data.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Data
{
    public class HotelContext : IdentityDbContext<IdentityUser>
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options)
        {
        }

        public DbSet<HabitacionEntidad> Habitaciones { get; set; }

        public DbSet<TipoHabitacionEntidad> TiposHabitacion { get; set; }

        public DbSet<ReservaEntidad> Reservas { get; set; }

        public DbSet<PaqueteEntidad> Paquetes { get; set; }

        public DbSet<PagoEntidad> Pagos { get; set; }

        public DbSet<AnulacionPagoEntidad> AnulacionesPago { get; set; }

     
        public DbSet<UsuarioEntidad> Usuarios { get; set; }
    }
}
