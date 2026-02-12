using FoodBooking.Domain.Common;
using FoodBooking.Domain.Enums;

namespace FoodBooking.Domain.Entities;

public class User : BaseEntity
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole UserRole { get; set; } = UserRole.Customer;
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
    public DateTime? LastLogin { get; set; }

    // Navigation properties
    // Phase B: Orders, Addresses
    // Phase C: Reviews
}