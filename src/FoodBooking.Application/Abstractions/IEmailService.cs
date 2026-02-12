namespace FoodBooking.Application.Abstractions;

public interface IEmailService
{
    Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default);
}