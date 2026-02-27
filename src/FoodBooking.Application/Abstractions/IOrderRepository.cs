using FoodBooking.Domain.Entities;

namespace FoodBooking.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerPhoneAsync(string customerPhone, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByShipperIdAsync(int? shipperId, string? status, CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
