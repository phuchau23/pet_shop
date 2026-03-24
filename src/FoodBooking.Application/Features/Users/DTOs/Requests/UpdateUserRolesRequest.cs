namespace FoodBooking.Application.Features.Users.DTOs.Requests;

public class UpdateUserRolesRequest
{
    public List<string> Roles { get; set; } = new();
}
