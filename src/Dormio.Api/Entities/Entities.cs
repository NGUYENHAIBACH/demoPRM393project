using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Dormio.Api.Entities;

// Tables 1-3 (Identity): AspNetUsers, AspNetRoles, AspNetUserRoles
public class AppUser : IdentityUser<Guid>
{
    [MaxLength(255)] public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AppRole : IdentityRole<Guid>
{
    public AppRole() { }
    public AppRole(string name) : base(name) { }
}

// Table 4
public class BoardingHouse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
}

// Table 5
public class Room
{
    public Guid Id { get; set; }
    public Guid BoardingHouseId { get; set; }
    public BoardingHouse BoardingHouse { get; set; } = null!;
    [Required, MaxLength(50)] public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    [Column(TypeName = "decimal(8,2)")] public decimal? Area { get; set; }
    public int MaxOccupants { get; set; } = 1;
    [Column(TypeName = "decimal(12,2)")] public decimal BasePrice { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<RoomService> RoomServices { get; set; } = new List<RoomService>();
}

// Table 6
public class Service
{
    public Guid Id { get; set; }
    public Guid BoardingHouseId { get; set; }
    public BoardingHouse BoardingHouse { get; set; } = null!;
    [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
    [Column(TypeName = "decimal(12,2)")] public decimal Price { get; set; }
    [Required, MaxLength(50)] public string Unit { get; set; } = string.Empty;
    public bool IsMetered { get; set; }
    public bool IsActive { get; set; } = true;
}

// Table 7 (M:N Room - Service)
public class RoomService
{
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
}

// Table 8
public class Contract
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    [Column(TypeName = "decimal(12,2)")] public decimal RentPrice { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal DepositAmount { get; set; }
    public int MonthlyPaymentDay { get; set; } = 5;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TenantContract> Tenants { get; set; } = new List<TenantContract>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

// Table 9 (M:N Contract - Tenant)
public class TenantContract
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public Guid TenantId { get; set; }
    public AppUser Tenant { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

// Table 10
public class MeterReading
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public int BillingMonth { get; set; }
    public int BillingYear { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal PreviousValue { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal CurrentValue { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Table 11
public class Invoice
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public int BillingMonth { get; set; }
    public int BillingYear { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

// Table 12 (the rent line has ServiceId = null)
public class InvoiceItem
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public Guid? ServiceId { get; set; }
    public Service? Service { get; set; }
    [Required, MaxLength(255)] public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(12,2)")] public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal Amount { get; set; }
}

// Table 13
public class Payment
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public Guid? PayerId { get; set; }
    public AppUser? Payer { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    [MaxLength(100)] public string? TransactionRef { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Table 14
public class Grievance
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public AppUser Tenant { get; set; } = null!;
    public Guid? RoomId { get; set; }
    public Room? Room { get; set; }
    public GrievanceType Type { get; set; } = GrievanceType.Complaint;
    [Required, MaxLength(255)] public string Title { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public GrievancePriority Priority { get; set; } = GrievancePriority.Medium;
    public GrievanceStatus Status { get; set; } = GrievanceStatus.Pending;
    public Guid? ResolvedById { get; set; }
    public AppUser? ResolvedBy { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
