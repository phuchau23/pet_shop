namespace FoodBooking.Application.Features.Auth.DTOs.Responses;

public class ProfileResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string UserRole { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
