namespace FoodBooking.Application.Abstractions.Auth;

public interface IGoogleTokenVerifier
{
    Task<GoogleTokenPayload> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed record GoogleTokenPayload(
    string Subject,
    string Email,
    string? Name,
    string? PictureUrl,
    bool EmailVerified
);