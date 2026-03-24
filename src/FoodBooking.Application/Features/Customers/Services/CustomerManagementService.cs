using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Customers.DTOs.Responses;
using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Customers.Services;

public class CustomerManagementService : ICustomerManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;

    public CustomerManagementService(
        IUserRepository userRepository,
        IOrderRepository orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users
            .Where(u => u.UserRole == UserRole.Customer)
            .Select(MapToCustomerResponse);
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(customerId, cancellationToken);
        if (user == null || user.UserRole != UserRole.Customer)
        {
            return null;
        }

        return MapToCustomerResponse(user);
    }

    public async Task<IEnumerable<CustomerOrderResponse>> GetCustomerOrdersAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(customerId, cancellationToken);
        if (user == null || user.UserRole != UserRole.Customer)
        {
            throw new KeyNotFoundException($"Customer with id {customerId} not found");
        }

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            return Enumerable.Empty<CustomerOrderResponse>();
        }

        var orders = await _orderRepository.GetByCustomerPhoneAsync(user.PhoneNumber, cancellationToken);
        return orders.Select(o => new CustomerOrderResponse
        {
            OrderId = o.Id,
            Status = o.Status,
            TotalPrice = o.TotalPrice,
            FinalAmount = o.FinalAmount,
            VoucherCode = o.VoucherCode,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt
        });
    }

    public async Task<CustomerSummaryResponse?> GetCustomerSummaryAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(customerId, cancellationToken);
        if (user == null || user.UserRole != UserRole.Customer)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            return new CustomerSummaryResponse
            {
                CustomerId = customerId
            };
        }

        var orders = (await _orderRepository.GetByCustomerPhoneAsync(user.PhoneNumber, cancellationToken)).ToList();

        var deliveredOrders = orders.Where(o => o.Status == "delivered");
        var totalSpent = deliveredOrders.Sum(o => o.FinalAmount ?? o.TotalPrice ?? 0);

        return new CustomerSummaryResponse
        {
            CustomerId = customerId,
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == "pending"),
            ConfirmedOrders = orders.Count(o => o.Status == "confirmed"),
            ShippingOrders = orders.Count(o => o.Status == "shipping"),
            DeliveredOrders = orders.Count(o => o.Status == "delivered"),
            CancelledOrders = orders.Count(o => o.Status == "cancelled"),
            TotalSpent = totalSpent,
            LastOrderAt = orders.OrderByDescending(o => o.CreatedAt).Select(o => (DateTime?)o.CreatedAt).FirstOrDefault()
        };
    }

    private static CustomerResponse MapToCustomerResponse(User user)
    {
        return new CustomerResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AccountStatus = user.AccountStatus,
            AccountStatusName = user.AccountStatus.ToString(),
            LastLogin = user.LastLogin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
