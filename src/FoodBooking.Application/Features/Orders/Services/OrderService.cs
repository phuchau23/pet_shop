using System.Linq;
using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Orders.DTOs.Requests;
using FoodBooking.Application.Features.Orders.DTOs.Responses;
using FoodBooking.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodBooking.Application.Features.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IConfiguration configuration,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        // Get shop coordinates from configuration
        var shopLatStr = _configuration["ShopSettings:Latitude"];
        var shopLngStr = _configuration["ShopSettings:Longitude"];
        var shopLat = double.TryParse(shopLatStr, out var lat) ? lat : 10.8506;
        var shopLng = double.TryParse(shopLngStr, out var lng) ? lng : 106.7749;

        var order = new Order
        {
            CustomerName = request.Customer.Name,
            CustomerPhone = request.Customer.Phone,
            AddressDetail = request.DeliveryAddress.AddressDetail,
            WardCode = request.DeliveryAddress.WardCode,
            DistrictCode = request.DeliveryAddress.DistrictCode,
            ProvinceCode = request.DeliveryAddress.ProvinceCode,
            FullAddress = request.DeliveryAddress.FullAddress,
            CustomerLat = request.DeliveryAddress.Lat,
            CustomerLng = request.DeliveryAddress.Lng,
            ShopLat = shopLat,
            ShopLng = shopLng,
            Status = "pending",
            TotalPrice = request.TotalPrice,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add order items
        foreach (var itemRequest in request.Items)
        {
            var orderItem = new OrderItem
            {
                ProductId = itemRequest.ProductId,
                ProductName = itemRequest.ProductName,
                UnitPrice = itemRequest.UnitPrice,
                Quantity = itemRequest.Quantity,
                Subtotal = itemRequest.Subtotal
            };
            order.OrderItems.Add(orderItem);
        }

        var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);
        return MapToResponse(createdOrder);
    }

    public async Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id, cancellationToken);
        return order == null ? null : MapToResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetByCustomerPhoneAsync(string customerPhone, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByCustomerPhoneAsync(customerPhone, cancellationToken);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetByShipperIdAsync(int? shipperId, string? status, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByShipperIdAsync(shipperId, status, cancellationToken);
        return orders.Select(MapToResponse);
    }

    public async Task<OrderResponse> UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {id} not found");
        }

        // Validate status transition
        ValidateStatusTransition(order.Status, request.Status);

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return MapToResponse(order);
    }

    public async Task<OrderResponse> AssignShipperAsync(int id, AssignShipperRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {id} not found");
        }

        order.ShipperId = request.ShipperId;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return MapToResponse(order);
    }

    private static void ValidateStatusTransition(string currentStatus, string newStatus)
    {
        // Valid transitions:
        // pending → confirmed → shipping → delivered
        // any → cancelled

        if (newStatus == "cancelled")
        {
            return; // Can cancel from any status
        }

        var validTransitions = new Dictionary<string, List<string>>
        {
            { "pending", new List<string> { "confirmed", "cancelled" } },
            { "confirmed", new List<string> { "shipping", "cancelled" } },
            { "shipping", new List<string> { "delivered", "cancelled" } },
            { "delivered", new List<string>() },
            { "cancelled", new List<string>() }
        };

        if (!validTransitions.ContainsKey(currentStatus))
        {
            throw new InvalidOperationException($"Invalid current status: {currentStatus}");
        }

        if (!validTransitions[currentStatus].Contains(newStatus))
        {
            throw new InvalidOperationException($"Invalid status transition from {currentStatus} to {newStatus}");
        }
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            FullAddress = order.FullAddress,
            ShopLat = order.ShopLat,
            ShopLng = order.ShopLng,
            CustomerLat = order.CustomerLat,
            CustomerLng = order.CustomerLng,
            ShipperId = order.ShipperId,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            Note = order.Note,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                Subtotal = oi.Subtotal
            }).ToList()
        };
    }
}
