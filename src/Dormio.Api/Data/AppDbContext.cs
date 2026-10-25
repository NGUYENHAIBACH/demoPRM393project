using Dormio.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dormio.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BoardingHouse> BoardingHouses => Set<BoardingHouse>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<RoomService> RoomServices => Set<RoomService>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<TenantContract> TenantContracts => Set<TenantContract>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Grievance> Grievances => Set<Grievance>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Room>().HasIndex(r => new { r.BoardingHouseId, r.RoomNumber }).IsUnique();
        b.Entity<RoomService>().HasKey(x => new { x.RoomId, x.ServiceId });
        b.Entity<TenantContract>().HasKey(x => new { x.ContractId, x.TenantId });
        b.Entity<MeterReading>().HasIndex(m => new { m.RoomId, m.ServiceId, m.BillingMonth, m.BillingYear }).IsUnique();
        b.Entity<Invoice>().HasIndex(i => new { i.ContractId, i.BillingMonth, i.BillingYear }).IsUnique();

        // Restrict by default to avoid SQL Server multiple cascade paths; never hard-delete financial data
        foreach (var fk in b.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())
                     .Where(f => f.DeleteBehavior == DeleteBehavior.Cascade
                                 && f.DeclaringEntityType.ClrType.Namespace == typeof(Room).Namespace))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        b.Entity<InvoiceItem>().HasOne(x => x.Invoice).WithMany(i => i.Items).OnDelete(DeleteBehavior.Cascade);
        b.Entity<RoomService>().HasOne(x => x.Room).WithMany(r => r.RoomServices).OnDelete(DeleteBehavior.Cascade);
    }
}
