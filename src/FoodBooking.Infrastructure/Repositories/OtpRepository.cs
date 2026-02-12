using FoodBooking.Application.Abstractions;
using FoodBooking.Domain.Entities;
using FoodBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBooking.Infrastructure.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly AppDbContext _context;

    public OtpRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OtpVerification?> GetValidOtpAsync(string email, string otpCode, CancellationToken cancellationToken = default)
    {
        return await _context.OtpVerifications
            .Where(o => o.Email == email 
                && o.OtpCode == otpCode 
                && !o.IsUsed 
                && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OtpVerification> CreateAsync(OtpVerification otp, CancellationToken cancellationToken = default)
    {
        _context.OtpVerifications.Add(otp);
        await _context.SaveChangesAsync(cancellationToken);
        return otp;
    }

    public async Task UpdateAsync(OtpVerification otp, CancellationToken cancellationToken = default)
    {
        _context.OtpVerifications.Update(otp);
        await _context.SaveChangesAsync(cancellationToken);
    }
}