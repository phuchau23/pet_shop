using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // Register - Email + Password required, PhoneNumber + FullName optional
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await authService.RegisterAsync(request, cancellationToken);
                return Results.Ok(ApiResponse<bool>.Success(result, "Registration successful. OTP sent to email. Please verify to activate account."));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<bool>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<bool>.Error(400, ex.Message));
            }
        })
        .WithName("Register")
        .WithSummary("Register new user")
        .WithDescription("Register with email (required) and password (required). Phone number and full name are optional. OTP will be sent to email for verification.")
        .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // Verify OTP - Verify và Login
        group.MapPost("/verify-otp", async (
            [FromBody] VerifyOtpRequest request,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await authService.VerifyOtpAsync(request, cancellationToken);
                return Results.Ok(ApiResponse<AuthResponse>.Success(result, "OTP verified successfully. Account activated and logged in."));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<AuthResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<AuthResponse>.Error(400, ex.Message));
            }
        })
        .WithName("VerifyOtp")
        .WithSummary("Verify OTP and login")
        .WithDescription("Verify OTP code to activate account and receive JWT token. User must register first.")
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status400BadRequest);

        // Send OTP (for resend or login with OTP)
        group.MapPost("/send-otp", async (
            [FromBody] SendOtpRequest request,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await authService.SendOtpAsync(request, cancellationToken);
                return Results.Ok(ApiResponse<bool>.Success(result, "OTP sent successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<bool>.Error(400, ex.Message));
            }
        })
        .WithName("SendOtp")
        .WithSummary("Send OTP to email")
        .WithDescription("Send a 6-digit OTP code to the specified email address")
        .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // Login with Email/Password
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await authService.LoginAsync(request, cancellationToken);
                return Results.Ok(ApiResponse<AuthResponse>.Success(result, "Login successful"));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<AuthResponse>.Error(400, ex.Message));
            }
        })
        .WithName("Login")
        .WithSummary("Login with email and password")
        .WithDescription("Authenticate user with email and password, receive JWT token")
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status400BadRequest);

        // Login with Google ID token
        group.MapPost("/google-login", async (
            [FromBody] GoogleLoginRequest request,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await authService.GoogleLoginAsync(request, cancellationToken);
                return Results.Ok(ApiResponse<AuthResponse>.Success(result, "Google login successful"));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<AuthResponse>.Error(400, ex.Message));
            }
        })
        .WithName("GoogleLogin")
        .WithSummary("Login with Google")
        .WithDescription("Authenticate user with Google ID token and receive JWT token")
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status400BadRequest);
    }
}