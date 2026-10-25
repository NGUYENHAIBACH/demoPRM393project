using System.ComponentModel.DataAnnotations;
using Dormio.Api.Entities;

namespace Dormio.Api.Dtos;

public record RegisterRequest(
    [Required, StringLength(255)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, RegularExpression(@"^(0|\+84)\d{9}$", ErrorMessage = "Invalid Vietnamese phone number")] string PhoneNumber,
    [Required, MinLength(8)] string Password,
    [Required] string Role);

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, Guid UserId, string FullName, string Email, string[] Roles);

public class BoardingHouseDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int RoomCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record BoardingHouseSaveRequest(
    [Required, StringLength(255)] string Name,
    [Required, StringLength(500)] string Address,
    string? Description,
    bool IsActive = true);

public class RoomDto
{
    public Guid Id { get; set; }
    public Guid BoardingHouseId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal? Area { get; set; }
    public int MaxOccupants { get; set; }
    public decimal BasePrice { get; set; }
    public RoomStatus Status { get; set; }
}

public record RoomSaveRequest(
    [Required, StringLength(50)] string RoomNumber,
    [Range(0, 100)] int Floor,
    [Range(1, 1000)] decimal? Area,
    [Range(1, 20)] int MaxOccupants,
    [Range(0, 1_000_000_000)] decimal BasePrice,
    RoomStatus Status = RoomStatus.Available);
