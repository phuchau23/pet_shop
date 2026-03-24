using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Users.DTOs.Requests;
using FoodBooking.Application.Features.Users.DTOs.Responses;
using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Users.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;

    public UserManagementService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(status))
        {
            return users.Select(MapToResponse);
        }

        if (!Enum.TryParse<AccountStatus>(status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Invalid status filter. Use Active, Inactive, or Banned");
        }

        return users
            .Where(u => u.AccountStatus == parsedStatus)
            .Select(MapToResponse);
    }

    public async Task<UserResponse?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user == null ? null : MapToResponse(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        var role = ParseRole(request.Role);
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new InvalidOperationException("Password must be at least 6 characters");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            UserRole = role,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(user, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {userId} not found");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
                if (existingUser != null && existingUser.UserId != userId)
                {
                    throw new InvalidOperationException("Email already exists");
                }
                user.Email = email;
            }
        }

        user.FullName = request.FullName ?? user.FullName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateUserStatusAsync(int userId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {userId} not found");

        if (!Enum.TryParse<AccountStatus>(request.Status, true, out var status))
        {
            throw new InvalidOperationException("Invalid status. Use Active, Inactive, or Banned");
        }

        user.AccountStatus = status;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public async Task<UserResponse> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {userId} not found");

        if (user.AccountStatus != AccountStatus.Active)
        {
            throw new InvalidOperationException("Only Active users can be deleted. Delete operation sets status from Active to Inactive");
        }

        user.AccountStatus = AccountStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = Enum.GetValues<UserRole>()
            .Select(r => new RoleResponse
            {
                Id = (int)r,
                Name = r.ToString()
            });

        return Task.FromResult(roles);
    }

    public async Task<UserRolesResponse?> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }

        return new UserRolesResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Roles = new List<string> { user.UserRole.ToString() }
        };
    }

    public async Task<UserRolesResponse> UpdateUserRolesAsync(int userId, UpdateUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id {userId} not found");

        var roleName = request.Roles.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new InvalidOperationException("At least one role is required");
        }

        user.UserRole = ParseRole(roleName);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserRolesResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Roles = new List<string> { user.UserRole.ToString() }
        };
    }

    private static UserRole ParseRole(string role)
    {
        if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
        {
            throw new InvalidOperationException("Invalid role. Use Customer, Admin, or Shipper");
        }

        return parsedRole;
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            UserRole = user.UserRole,
            UserRoleName = user.UserRole.ToString(),
            AccountStatus = user.AccountStatus,
            AccountStatusName = user.AccountStatus.ToString(),
            LastLogin = user.LastLogin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
