using FoodBooking.Application.Abstractions;
using FoodBooking.Domain.Entities;
using FoodBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBooking.Infrastructure.Repositories;

public class VoucherRepository : IVoucherRepository
{
    private readonly AppDbContext _context;

    public VoucherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Voucher?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Vouchers
            .Where(v => v.IsActive
                && (v.StartDate == null || v.StartDate <= now)
                && (v.EndDate == null || v.EndDate >= now)
                && (v.UsageLimit == null || v.UsedCount < v.UsageLimit))
            .OrderBy(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Voucher> CreateAsync(Voucher voucher, CancellationToken cancellationToken = default)
    {
        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync(cancellationToken);
        return voucher;
    }

    public async Task<Voucher> UpdateAsync(Voucher voucher, CancellationToken cancellationToken = default)
    {
        _context.Vouchers.Update(voucher);
        await _context.SaveChangesAsync(cancellationToken);
        return voucher;
    }

    public async Task IncrementUsageCountAsync(int voucherId, CancellationToken cancellationToken = default)
    {
        var voucher = await _context.Vouchers.FindAsync(new object[] { voucherId }, cancellationToken);
        if (voucher != null)
        {
            voucher.UsedCount++;
            voucher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
