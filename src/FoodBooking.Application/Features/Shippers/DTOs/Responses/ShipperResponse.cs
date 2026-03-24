using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Shippers.DTOs.Responses;

public class ShipperResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public string AccountStatusName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
