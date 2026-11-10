using HotelReservas.Domain.Entities;
using HotelReservas.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelReservas.Persistence.Context;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<Hotel> Hoteles => Set<Hotel>();
    public DbSet<Bloque> Bloques => Set<Bloque>();
    public DbSet<Piso> Pisos => Set<Piso>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<TipoHabitacion> TiposHabitacion => Set<TipoHabitacion>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Hotel>(e =>
        {
            e.ToTable("Hotel");
            e.HasKey(x => x.IdHotel);
            e.HasIndex(x => x.Nombre).IsUnique();
        });

        mb.Entity<Bloque>(e =>
        {
            e.ToTable("Bloque");
            e.HasKey(x => x.IdBloque);
            e.HasIndex(x => new { x.IdHotel, x.Codigo }).IsUnique();
            e.HasOne(x => x.Hotel).WithMany(h => h.Bloques)
             .HasForeignKey(x => x.IdHotel).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Piso>(e =>
        {
            e.ToTable("Piso");
            e.HasKey(x => x.IdPiso);
            e.HasOne(x => x.Hotel).WithMany(h => h.Pisos)
             .HasForeignKey(x => x.IdHotel).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Bloque).WithMany(b => b.Pisos)
             .HasForeignKey(x => x.IdBloque).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<TipoHabitacion>(e =>
        {
            e.ToTable("TipoHabitacion");
            e.HasKey(x => x.IdTipoHabitacion);
            e.HasIndex(x => x.Nombre).IsUnique();
            e.Property(x => x.TarifaBase).HasPrecision(12, 2);
        });

        mb.Entity<Habitacion>(e =>
        {
            e.ToTable("Habitacion");
            e.HasKey(x => x.IdHabitacion);
            e.Ignore(x => x.EsReservable);
            e.Property(x => x.Tarifa).HasPrecision(12, 2);
            e.HasOne(x => x.Piso).WithMany(p => p.Habitaciones)
             .HasForeignKey(x => x.IdPiso).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TipoHabitacion).WithMany(t => t.Habitaciones)
             .HasForeignKey(x => x.IdTipoHabitacion).OnDelete(DeleteBehavior.Restrict);
        });

        Seed(mb);
    }

    private static void Seed(ModelBuilder mb)
    {
        var fecha = new DateTime(2026, 1, 1);

        mb.Entity<TipoHabitacion>().HasData(
            new TipoHabitacion { IdTipoHabitacion = 1, Nombre = "Sencilla", Descripcion = "Una cama individual", CapacidadMaxima = 1, TarifaBase = 2500m },
            new TipoHabitacion { IdTipoHabitacion = 2, Nombre = "Doble", Descripcion = "Dos camas o una cama matrimonial", CapacidadMaxima = 2, TarifaBase = 3800m },
            new TipoHabitacion { IdTipoHabitacion = 3, Nombre = "Familiar", Descripcion = "Habitación amplia para familias", CapacidadMaxima = 4, TarifaBase = 5500m },
            new TipoHabitacion { IdTipoHabitacion = 4, Nombre = "Suite", Descripcion = "Suite con sala y vista al mar", CapacidadMaxima = 3, TarifaBase = 9000m });

        mb.Entity<Hotel>().HasData(
            new Hotel { IdHotel = 1, Nombre = "Hotel Caribe Santo Domingo", Direccion = "Av. George Washington #100, Santo Domingo", Telefono = "809-555-0101", Correo = "sd@caribe.com", FechaRegistro = fecha },
            new Hotel { IdHotel = 2, Nombre = "Hotel Caribe Punta Cana", Direccion = "Playa Bávaro, La Altagracia", Telefono = "809-555-0202", Correo = "pc@caribe.com", FechaRegistro = fecha });

        mb.Entity<Bloque>().HasData(
            new Bloque { IdBloque = 1, IdHotel = 1, Codigo = "A", Nombre = "Torre A", Descripcion = "Torre principal" },
            new Bloque { IdBloque = 2, IdHotel = 1, Codigo = "B", Nombre = "Torre B", Descripcion = "Torre ejecutiva" },
            new Bloque { IdBloque = 3, IdHotel = 2, Codigo = "V", Nombre = "Villas", Descripcion = "Villas frente al mar" });

        mb.Entity<Piso>().HasData(
            new Piso { IdPiso = 1, IdHotel = 1, IdBloque = 1, Numero = 1, Descripcion = "Planta baja" },
            new Piso { IdPiso = 2, IdHotel = 1, IdBloque = 1, Numero = 2 },
            new Piso { IdPiso = 3, IdHotel = 1, IdBloque = 2, Numero = 1 },
            new Piso { IdPiso = 4, IdHotel = 2, IdBloque = 3, Numero = 1 });

        mb.Entity<Habitacion>().HasData(
            new Habitacion { IdHabitacion = 1, IdPiso = 1, IdTipoHabitacion = 1, Numero = "A101", Capacidad = 1, Tarifa = 2500m },
            new Habitacion { IdHabitacion = 2, IdPiso = 1, IdTipoHabitacion = 2, Numero = "A102", Capacidad = 2, Tarifa = 3800m },
            new Habitacion { IdHabitacion = 3, IdPiso = 2, IdTipoHabitacion = 4, Numero = "A201", Capacidad = 3, Tarifa = 9000m, Estado = EstadoHabitacion.Mantenimiento },
            new Habitacion { IdHabitacion = 4, IdPiso = 3, IdTipoHabitacion = 2, Numero = "B101", Capacidad = 2, Tarifa = 4000m },
            new Habitacion { IdHabitacion = 5, IdPiso = 4, IdTipoHabitacion = 3, Numero = "V01", Capacidad = 4, Tarifa = 6500m });
    }
}
