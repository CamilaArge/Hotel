using HotelLosPatitos.Data.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelLosPatitos.Data
{
    // Contexto de la base de datos "Proyecto".
    // Hereda de IdentityDbContext para manejar también los usuarios y roles del login (tablas AspNet...)
    public class HotelContext : IdentityDbContext<IdentityUser>
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options)
        {
        }

        public DbSet<HabitacionEntidad> Habitaciones { get; set; }
        public DbSet<ReservaEntidad> Reservas { get; set; }
    }
}
