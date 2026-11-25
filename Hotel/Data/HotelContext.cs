using Hotel.Data.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Data
{
    public class HotelContext : IdentityDbContext<IdentityUser>
    {
        public HotelContext(DbContextOptions<HotelContext> options)
            : base(options)
        {
        }
        public DbSet<HabitacionEntidad> Habitaciones { get; set; }
        public DbSet<TipoHabitacionEntidad> TiposHabitacion { get; set; }
        public DbSet<ReservaEntidad> Reservas { get; set; }
        public DbSet<UsuarioEntidad> Usuarios { get; set; }
        public DbSet<PaqueteEntidad> Paquetes { get; set; }
        public DbSet<PagoEntidad> Pagos { get; set; }
        public DbSet<AnulacionPagoEntidad> AnulacionesPago { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relación entre Usuario e Identity
            builder.Entity<UsuarioEntidad>()
                .HasIndex(u => u.IdentityId)
                .IsUnique()
                .HasFilter("[IdentityId] IS NOT NULL");

            builder.Entity<UsuarioEntidad>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<UsuarioEntidad>(u => u.IdentityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario - Reserva
            builder.Entity<ReservaEntidad>()
                .HasOne<UsuarioEntidad>()
                .WithMany()
                .HasForeignKey(r => r.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario - Pago
            builder.Entity<PagoEntidad>()
                .HasOne<UsuarioEntidad>()
                .WithMany()
                .HasForeignKey(p => p.IdUsuarioRegistro)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario - Anulación de pago
            builder.Entity<AnulacionPagoEntidad>()
                .HasOne<UsuarioEntidad>()
                .WithMany()
                .HasForeignKey(a => a.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            // Pago - Anulación de pago
            builder.Entity<AnulacionPagoEntidad>()
                .HasOne<PagoEntidad>()
                .WithMany()
                .HasForeignKey(a => a.IdPago)
                .OnDelete(DeleteBehavior.Restrict);

            // Reserva - Pago
            builder.Entity<PagoEntidad>()
                .HasOne<ReservaEntidad>()
                .WithMany()
                .HasForeignKey(p => p.IdReserva)
                .OnDelete(DeleteBehavior.Restrict);

            // Reserva - Habitación
            builder.Entity<ReservaEntidad>()
                .HasOne<HabitacionEntidad>()
                .WithMany()
                .HasForeignKey(r => r.IdHabitacion)
                .OnDelete(DeleteBehavior.Restrict);

            // Reserva - Paquete
            builder.Entity<ReservaEntidad>()
                .HasOne<PaqueteEntidad>()
                .WithMany()
                .HasForeignKey(r => r.IdPaquete)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
