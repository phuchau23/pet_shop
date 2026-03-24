namespace FoodBooking.Application.Features.Users.DTOs.Responses;

public class UserRolesResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
