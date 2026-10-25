namespace Dormio.Api.Entities;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Landlord = "Landlord";
    public const string Tenant = "Tenant";
    public static readonly string[] All = { Admin, Landlord, Tenant };
}

public enum RoomStatus { Available, Occupied, Maintenance }
public enum ContractStatus { Draft, Active, Expired, Canceled }
public enum InvoiceStatus { Unpaid, Paid, Overdue }
public enum PaymentMethod { Cash, Banking }
public enum PaymentStatus { Pending, Success, Failed }
public enum GrievanceType { Complaint, Inquiry, Feedback }
public enum GrievancePriority { Low, Medium, High }
public enum GrievanceStatus { Pending, InProgress, Resolved, Rejected }
