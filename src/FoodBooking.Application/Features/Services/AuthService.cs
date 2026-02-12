using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Auth.DTOs;
using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace FoodBooking.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
{
    // Check if user already exists
    var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
    if (existingUser != null)
    {
        throw new InvalidOperationException("Email already registered");
    }

    // Create user with Inactive status (will be activated after OTP verification)
    var user = new User
    {
        Email = request.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        PhoneNumber = request.PhoneNumber,
        FullName = request.FullName,
        UserRole = UserRole.Customer,
        AccountStatus = AccountStatus.Inactive, // Inactive until OTP verified
        LastLogin = null
    };

    await _userRepository.CreateAsync(user, cancellationToken);

    // Generate 6-digit OTP
    var otpCode = new Random().Next(100000, 999999).ToString();

    // Save OTP to database
    var otpVerification = new OtpVerification
    {
        Email = request.Email,
        OtpCode = otpCode,
        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
        IsUsed = false
    };

    await _otpRepository.CreateAsync(otpVerification, cancellationToken);

    // Send OTP email
    var emailSent = await _emailService.SendOtpEmailAsync(request.Email, otpCode, cancellationToken);

    if (!emailSent)
    {
        _logger.LogWarning("Failed to send OTP email to {Email}", request.Email);
    }

    return emailSent;
}

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (user.AccountStatus != AccountStatus.Active)
        {
            throw new UnauthorizedAccessException("Account is not active");
        }

        // Update last login
        user.LastLogin = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = new UserInfo
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                UserRole = user.UserRole.ToString()
            }
        };
    }

    public async Task<bool> SendOtpAsync(SendOtpRequest request, CancellationToken cancellationToken = default)
    {
        // Generate 6-digit OTP
        var otpCode = new Random().Next(100000, 999999).ToString();

        // Save OTP to database
        var otpVerification = new OtpVerification
        {
            Email = request.Email,
            OtpCode = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        await _otpRepository.CreateAsync(otpVerification, cancellationToken);

        // Send email
        var emailSent = await _emailService.SendOtpEmailAsync(request.Email, otpCode, cancellationToken);

        if (!emailSent)
        {
            _logger.LogWarning("Failed to send OTP email to {Email}", request.Email);
        }

        return emailSent;
    }

    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
{
    var otp = await _otpRepository.GetValidOtpAsync(request.Email, request.OtpCode, cancellationToken);

    if (otp == null)
    {
        throw new UnauthorizedAccessException("Invalid or expired OTP");
    }

    // Mark OTP as used
    otp.IsUsed = true;
    await _otpRepository.UpdateAsync(otp, cancellationToken);

    // Get user (should exist from Register step)
    var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

    if (user == null)
    {
        throw new InvalidOperationException("User not found. Please register first.");
    }

    // Activate user and update last login
    if (user.AccountStatus == AccountStatus.Inactive)
    {
        user.AccountStatus = AccountStatus.Active;
    }
    
    user.LastLogin = DateTime.UtcNow;
    await _userRepository.UpdateAsync(user, cancellationToken);

    // Generate token
    var token = GenerateJwtToken(user);
    var refreshToken = GenerateRefreshToken();

    return new AuthResponse
    {
        Token = token,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddHours(24),
        User = new UserInfo
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            UserRole = user.UserRole.ToString()
        }
    };
}

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.UserRole.ToString()),
            new Claim("UserId", user.UserId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"] ?? "FoodBooking",
            audience: _configuration["JwtSettings:Audience"] ?? "FoodBooking",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}