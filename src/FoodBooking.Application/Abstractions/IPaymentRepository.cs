using FoodBooking.Domain.Entities;

namespace FoodBooking.Application.Abstractions;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByTransactionRefAsync(string transactionRef, CancellationToken cancellationToken = default);
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
