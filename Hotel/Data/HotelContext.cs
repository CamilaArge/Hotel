using HotelLosPatitos.Data.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelLosPatitos.Data
{

    public class HotelContext : IdentityDbContext<IdentityUser>
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options)
        {
        }

        public DbSet<HabitacionEntidad> Habitaciones { get; set; }
        public DbSet<ReservaEntidad> Reservas { get; set; }
    }
}
