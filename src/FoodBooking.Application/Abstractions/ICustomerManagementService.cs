using FoodBooking.Application.Features.Customers.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface ICustomerManagementService
{
    Task<IEnumerable<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<CustomerResponse?> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerOrderResponse>> GetCustomerOrdersAsync(int customerId, CancellationToken cancellationToken = default);
    Task<CustomerSummaryResponse?> GetCustomerSummaryAsync(int customerId, CancellationToken cancellationToken = default);
}
