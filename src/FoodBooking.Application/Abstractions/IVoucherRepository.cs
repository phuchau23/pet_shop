using FoodBooking.Domain.Entities;

namespace FoodBooking.Application.Abstractions;

public interface IVoucherRepository
{
    Task<Voucher?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Voucher>> GetActiveVouchersAsync(CancellationToken cancellationToken = default);
    Task<Voucher> CreateAsync(Voucher voucher, CancellationToken cancellationToken = default);
    Task<Voucher> UpdateAsync(Voucher voucher, CancellationToken cancellationToken = default);
    Task DeleteAsync(Voucher voucher, CancellationToken cancellationToken = default);
    Task IncrementUsageCountAsync(int voucherId, CancellationToken cancellationToken = default);
}
