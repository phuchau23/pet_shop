using FoodBooking.Application.Features.Users.DTOs.Requests;
using FoodBooking.Application.Features.Users.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface IUserManagementService
{
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateUserStatusAsync(int userId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<UserRolesResponse?> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserRolesResponse> UpdateUserRolesAsync(int userId, UpdateUserRolesRequest request, CancellationToken cancellationToken = default);
}
