using FoodBooking.Application.Abstractions.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoodBooking.Infrastructure.External.Google;

public class GoogleTokenVerifier : IGoogleTokenVerifier
{
    private readonly string[] _audiences;
    private readonly bool _allowAudienceBypass;
    private readonly ILogger<GoogleTokenVerifier> _logger;

    public GoogleTokenVerifier(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        ILogger<GoogleTokenVerifier> logger)
    {
        _logger = logger;

        var audienceList = new List<string>();

        var singleClientId = configuration["GoogleAuth:ClientId"];
        if (!string.IsNullOrWhiteSpace(singleClientId))
        {
            audienceList.Add(singleClientId);
        }

        var multipleClientIds = configuration.GetSection("GoogleAuth:ClientIds").Get<string[]>();
        if (multipleClientIds is { Length: > 0 })
        {
            audienceList.AddRange(multipleClientIds);
        }

        _audiences = audienceList
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => !x.Contains("YOUR_GOOGLE_CLIENT_ID", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        _allowAudienceBypass = hostEnvironment.IsDevelopment();

        if (_audiences.Length == 0)
        {
            if (_allowAudienceBypass)
            {
                _logger.LogWarning("GoogleAuth ClientId is not configured correctly. Audience validation is temporarily disabled in Development only.");
            }
            else
            {
                throw new InvalidOperationException("GoogleAuth ClientId is missing or invalid. Configure GoogleAuth:ClientId (or ClientIds) for non-development environments.");
            }
        }
    }

    public async Task<GoogleTokenPayload> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = _audiences.Length > 0 ? _audiences : (_allowAudienceBypass ? null : _audiences)
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

        return new GoogleTokenPayload(
            payload.Subject,
            payload.Email,
            payload.Name,
            payload.Picture,
            payload.EmailVerified);
    }
}