using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Customers.DTOs.Responses;

public class CustomerResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public string AccountStatusName { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
