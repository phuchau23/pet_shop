using FoodBooking.Domain.Entities;

namespace FoodBooking.Application.Abstractions;

public interface IOtpRepository
{
    Task<OtpVerification?> GetValidOtpAsync(string email, string otpCode, CancellationToken cancellationToken = default);
    Task<OtpVerification> CreateAsync(OtpVerification otp, CancellationToken cancellationToken = default);
    Task UpdateAsync(OtpVerification otp, CancellationToken cancellationToken = default);
}