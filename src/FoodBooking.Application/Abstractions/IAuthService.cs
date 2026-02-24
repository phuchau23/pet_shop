using FoodBooking.Application.Features.Auth.DTOs;

namespace FoodBooking.Application.Abstractions;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> SendOtpAsync(SendOtpRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
}