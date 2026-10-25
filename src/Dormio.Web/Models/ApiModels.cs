using System.ComponentModel.DataAnnotations;

namespace Dormio.Web.Models;

public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}

public class RegisterVm
{
    [Required, Display(Name = "Full name")] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, Display(Name = "Phone number"), RegularExpression(@"^(0|\+84)\d{9}$", ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = "Tenant";
}

public record AuthResponseVm(string Token, DateTime ExpiresAt, Guid UserId, string FullName, string Email, string[] Roles);

public class BoardingHouseVm
{
    public Guid Id { get; set; }
    [Required, StringLength(255)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(500)] public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Display(Name = "Active")] public bool IsActive { get; set; } = true;
    public int RoomCount { get; set; }
}
