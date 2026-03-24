namespace FoodBooking.Application.Features.Users.DTOs.Requests;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "Customer";
}
