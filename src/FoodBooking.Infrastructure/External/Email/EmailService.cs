using FoodBooking.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace FoodBooking.Infrastructure.External.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailService(
        IConfiguration configuration, 
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["EmailSettings:ResendApiKey"] 
                ?? throw new InvalidOperationException("Resend API Key not configured");
            
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "onboarding@resend.dev";
            var fromName = _configuration["EmailSettings:FromName"] ?? "FoodBooking";

            var requestBody = new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { toEmail },
                subject = "Your OTP Code - FoodBooking",
                html = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>OTP Verification Code</h2>
                        <p>Your OTP code is:</p>
                        <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0;'>
                            <h1 style='color: #007bff; font-size: 32px; letter-spacing: 5px; margin: 0;'>{otpCode}</h1>
                        </div>
                        <p>This code will expire in 10 minutes.</p>
                        <p style='color: #666; font-size: 12px;'>If you didn't request this code, please ignore this email.</p>
                    </div>"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await httpClient.PostAsync("https://api.resend.com/emails", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("OTP email sent successfully to {Email}. Response: {Response}", toEmail, responseContent);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send OTP email to {Email}. Status: {Status}, Error: {Error}", 
                    toEmail, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
            return false;
        }
    }
}