using System.Linq;
using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Orders.DTOs.Requests;
using FoodBooking.Application.Features.Orders.DTOs.Responses;
using FoodBooking.Application.Features.Vouchers.Services;
using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoodBooking.Application.Features.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderService> _logger;
    private readonly IRoutingService _routingService;
    private readonly IVoucherService _voucherService;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IPaymentService _paymentService;

    public OrderService(
        IOrderRepository orderRepository,
        IConfiguration configuration,
        ILogger<OrderService> logger,
        IRoutingService routingService,
        IVoucherService voucherService,
        IVoucherRepository voucherRepository,
        IPaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _configuration = configuration;
        _logger = logger;
        _routingService = routingService;
        _voucherService = voucherService;
        _voucherRepository = voucherRepository;
        _paymentService = paymentService;
    }

    public async Task<EstimateDeliveryResponse> EstimateDeliveryTimeAsync(EstimateDeliveryRequest request, CancellationToken cancellationToken = default)
    {
        // Get shop coordinates from configuration
        var shopLatStr = _configuration["ShopSettings:Latitude"];
        var shopLngStr = _configuration["ShopSettings:Longitude"];
        var shopLat = double.TryParse(shopLatStr, out var lat) ? lat : 10.841449;
        var shopLng = double.TryParse(shopLngStr, out var lng) ? lng : 106.809997;

        // Call OSRM to get route info
        var routeInfo = await _routingService.GetRouteInfoAsync(
            shopLat, shopLng, 
            request.CustomerLat, request.CustomerLng, 
            cancellationToken);

        double distanceMeters;
        double distanceKm;
        int estimatedMinutes;

        if (routeInfo == null)
        {
            // Fallback: calculate simple distance-based estimate
            distanceKm = CalculateDistance(shopLat, shopLng, request.CustomerLat, request.CustomerLng);
            distanceMeters = distanceKm * 1000;
            estimatedMinutes = (int)Math.Ceiling(distanceKm * 3); // Assume 3 minutes per km
        }
        else
        {
            distanceMeters = routeInfo.DistanceMeters;
            distanceKm = Math.Round(routeInfo.DistanceMeters / 1000.0, 2);
            estimatedMinutes = routeInfo.EstimatedMinutes;
        }

        // Tính phí ship dựa trên khoảng cách và tổng tiền đơn hàng
        var deliveryFee = CalculateDeliveryFee(distanceMeters, request.OrderTotal);

        return new EstimateDeliveryResponse
        {
            ShopLat = shopLat,
            ShopLng = shopLng,
            ShopName = "Đại Học FPT University",
            CustomerLat = request.CustomerLat,
            CustomerLng = request.CustomerLng,
            EstimatedDeliveryMinutes = estimatedMinutes,
            EstimatedDistanceMeters = distanceMeters,
            EstimatedDistanceKm = distanceKm,
            DeliveryFee = deliveryFee,
            RouteCoordinates = routeInfo?.RouteCoordinates
        };
    }

    // Haversine formula to calculate distance between two coordinates
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    /// <summary>
    /// Tính phí ship dựa trên khoảng cách và tổng tiền đơn hàng
    /// </summary>
    private decimal CalculateDeliveryFee(double? distanceMeters, decimal? orderTotal = null)
    {
        if (distanceMeters == null || distanceMeters <= 0)
            return 0;

        var distanceKm = distanceMeters.Value / 1000.0;
        var baseFeeStr = _configuration["DeliverySettings:BaseFee"];
        var baseFee = decimal.TryParse(baseFeeStr, out var bf) ? bf : 15000;
        var feePerKmStr = _configuration["DeliverySettings:FeePerKm"];
        var feePerKm = decimal.TryParse(feePerKmStr, out var fpk) ? fpk : 5000;
        var freeThresholdStr = _configuration["DeliverySettings:FreeDeliveryThreshold"];
        var freeThreshold = decimal.TryParse(freeThresholdStr, out var ft) ? ft : 500000;
        var maxDistanceKmStr = _configuration["DeliverySettings:MaxDistanceKm"];
        var maxDistanceKm = double.TryParse(maxDistanceKmStr, out var md) ? md : 1000;

        // Kiểm tra khoảng cách tối đa
        if (distanceKm > maxDistanceKm)
        {
            throw new InvalidOperationException($"Khoảng cách giao hàng vượt quá giới hạn {maxDistanceKm}km");
        }

        // Miễn ship nếu đơn hàng >= threshold
        if (orderTotal.HasValue && orderTotal.Value >= freeThreshold)
        {
            return 0;
        }

        // Tính phí: BaseFee + (Km * FeePerKm)
        var deliveryFee = baseFee + (decimal)distanceKm * feePerKm;

        return Math.Round(deliveryFee, 0);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        // Get shop coordinates from configuration
        var shopLatStr = _configuration["ShopSettings:Latitude"];
        var shopLngStr = _configuration["ShopSettings:Longitude"];
        var shopLat = double.TryParse(shopLatStr, out var lat) ? lat : 10.841449;
        var shopLng = double.TryParse(shopLngStr, out var lng) ? lng : 106.809997;

        // Calculate estimated delivery time if coordinates are provided
        int? estimatedMinutes = null;
        double? estimatedDistance = null;
        
        if (request.DeliveryAddress.Lat.HasValue && request.DeliveryAddress.Lng.HasValue)
        {
            var routeInfo = await _routingService.GetRouteInfoAsync(
                shopLat, shopLng,
                request.DeliveryAddress.Lat.Value, request.DeliveryAddress.Lng.Value,
                cancellationToken);

            if (routeInfo != null)
            {
                estimatedMinutes = routeInfo.EstimatedMinutes;
                estimatedDistance = routeInfo.DistanceMeters;
            }
        }

        // Apply voucher if provided
        decimal voucherDiscount = 0;
        decimal finalAmount = request.TotalPrice;
        int? voucherId = null;
        string? voucherCode = null;

        if (!string.IsNullOrWhiteSpace(request.VoucherCode))
        {
            try
            {
                var voucherResponse = await _voucherService.ValidateAndGetVoucherAsync(
                    request.VoucherCode, 
                    request.TotalPrice, 
                    cancellationToken);
                
                if (voucherResponse != null)
                {
                    var voucher = await _voucherRepository.GetByIdAsync(voucherResponse.Id, cancellationToken);
                    if (voucher != null)
                    {
                        voucherDiscount = VoucherService.CalculateDiscount(voucher, request.TotalPrice);
                        finalAmount = request.TotalPrice - voucherDiscount;
                        voucherId = voucher.Id;
                        voucherCode = voucher.Code;
                        
                        // Increment usage count
                        await _voucherRepository.IncrementUsageCountAsync(voucher.Id, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply voucher {VoucherCode}: {Message}", request.VoucherCode, ex.Message);
                throw new InvalidOperationException($"Không thể áp dụng voucher: {ex.Message}");
            }
        }

        // Determine initial order status based on payment method
        // COD: pending (chưa trả tiền), other methods: pending (chờ thanh toán)
        var initialStatus = "pending";

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
            EstimatedDeliveryMinutes = estimatedMinutes,
            EstimatedDistanceMeters = estimatedDistance,
            Status = initialStatus,
            TotalPrice = request.TotalPrice,
            VoucherDiscount = voucherDiscount > 0 ? voucherDiscount : null,
            FinalAmount = finalAmount,
            VoucherId = voucherId,
            VoucherCode = voucherCode,
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

        // Create payment if payment method is specified
        if (request.PaymentMethod.HasValue)
        {
            try
            {
                var createPaymentRequest = new Application.Features.Payments.DTOs.Requests.CreatePaymentRequest
                {
                    OrderId = createdOrder.Id,
                    PaymentMethod = request.PaymentMethod.Value,
                    ClientIpAddress = request.ClientIpAddress
                };
                
                await _paymentService.CreatePaymentAsync(createPaymentRequest, cancellationToken);
                
                // Update order status based on payment status
                // For COD, status remains "pending" until delivered
                // For other methods, status will be updated when payment is confirmed
                var payment = await _paymentService.GetByOrderIdAsync(createdOrder.Id, cancellationToken);
                if (payment != null && payment.Status == Domain.Enums.PaymentStatus.Paid)
                {
                    createdOrder.Status = "confirmed";
                    await _orderRepository.UpdateAsync(createdOrder, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment for order {OrderId}", createdOrder.Id);
                if (request.PaymentMethod == PaymentMethod.VNPay)
                {
                    throw new InvalidOperationException("Order created but failed to initialize VNPay payment. Please retry payment creation.", ex);
                }
            }
        }

        var orderWithPayment = await _orderRepository.GetByIdWithItemsAsync(createdOrder.Id, cancellationToken);
        return MapToResponse(orderWithPayment ?? createdOrder);
    }

    public async Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id, cancellationToken);
        return order == null ? null : MapToResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(string customerPhone, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByCustomerPhoneAsync(customerPhone, cancellationToken);
        return orders.Select(MapToResponse);
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

    public async Task<IEnumerable<OrderResponse>> GetShipperOrdersAsync(int shipperId, string? status, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByShipperIdAsync(shipperId, status, cancellationToken);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetAvailableOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAvailableOrdersAsync(cancellationToken);
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

    public async Task<OrderTrackingResponse> GetTrackingAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {id} not found");
        }

        return BuildTrackingResponse(order);
    }

    public async Task UpdateShipperLocationAsync(int orderId, int shipperId, double lat, double lng, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {orderId} not found");
        }

        if (order.ShipperId != shipperId)
        {
            throw new UnauthorizedAccessException($"Shipper {shipperId} is not assigned to order {orderId}");
        }

        order.ShipperCurrentLat = lat;
        order.ShipperCurrentLng = lng;
        order.ShipperLocationUpdatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
    }

    public async Task<OrderResponse> UpdateShipperStatusAsync(int id, UpdateShipperStatusRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {id} not found");
        }

        // Validate status transition
        ValidateStatusTransition(order.Status, request.Status);

        // Nếu shipper chấp nhận đơn (từ pending/confirmed → shipping) và order chưa có shipperId
        // thì tự động gán shipperId vào order
        if ((order.Status == "pending" || order.Status == "confirmed") && request.Status == "shipping")
        {
            if (order.ShipperId == null)
            {
                // Shipper tự nhận đơn - gán shipperId vào order
                order.ShipperId = request.ShipperId;
            }
            else if (order.ShipperId != request.ShipperId)
            {
                // Order đã được gán cho shipper khác
                throw new UnauthorizedAccessException($"Order {id} has already been assigned to shipper {order.ShipperId}");
            }
            
            // Khi shipper nhận đơn, lưu vị trí ban đầu của shipper
            if (request.Lat.HasValue && request.Lng.HasValue)
            {
                order.ShipperCurrentLat = request.Lat.Value;
                order.ShipperCurrentLng = request.Lng.Value;
                order.ShipperLocationUpdatedAt = DateTime.UtcNow;
            }
        }
        else if (request.Status == "delivered")
        {
            // Với status delivered, phải validate shipperId
            if (order.ShipperId == null)
            {
                throw new InvalidOperationException($"Order {id} must have a shipper assigned before updating to status 'delivered'");
            }
            
            if (order.ShipperId != request.ShipperId)
            {
                throw new UnauthorizedAccessException($"Shipper {request.ShipperId} is not assigned to order {id}");
            }
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return MapToResponse(order);
    }

    private static void ValidateStatusTransition(string currentStatus, string newStatus)
    {
        // Valid transitions:
        // pending → shipping (shipper nhận đơn trực tiếp) hoặc pending → confirmed → shipping
        // pending → confirmed → shipping → delivered
        // any → cancelled

        if (newStatus == "cancelled")
        {
            return; // Can cancel from any status
        }

        var validTransitions = new Dictionary<string, List<string>>
        {
            { "pending", new List<string> { "confirmed", "shipping", "cancelled" } }, // Cho phép pending → shipping (shipper nhận trực tiếp)
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

    private OrderResponse MapToResponse(Order order)
    {
        // Tính phí ship dựa trên khoảng cách và tổng tiền đơn hàng
        decimal? deliveryFee = null;
        if (order.EstimatedDistanceMeters.HasValue && order.EstimatedDistanceMeters.Value > 0)
        {
            try
            {
                deliveryFee = CalculateDeliveryFee(order.EstimatedDistanceMeters.Value, order.TotalPrice);
            }
            catch (InvalidOperationException ex)
            {
                // Không để lỗi "vượt quá giới hạn km" làm gãy toàn bộ endpoint list đơn.
                // Với response, chỉ trả deliveryFee = null (FE tự xử lý hiển thị).
                _logger.LogWarning(ex, "Skip delivery fee calculation for order {OrderId} because distance is out of range. distanceMeters={DistanceMeters}", order.Id, order.EstimatedDistanceMeters);
                deliveryFee = null;
            }
        }

        // Lấy PaymentMethod từ Payment navigation property
        PaymentMethod? paymentMethod = order.Payment?.PaymentMethod;
        string? paymentUrl = null;
        if (!string.IsNullOrWhiteSpace(order.Payment?.PaymentMetadata))
        {
            try
            {
                using var doc = JsonDocument.Parse(order.Payment.PaymentMetadata);
                if (doc.RootElement.TryGetProperty("paymentUrl", out var paymentUrlElement))
                {
                    paymentUrl = paymentUrlElement.GetString();
                }
            }
            catch
            {
                // Ignore metadata parse error and keep paymentUrl null.
            }
        }

        return new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            FullAddress = order.FullAddress,
            ShopLat = order.ShopLat,
            ShopLng = order.ShopLng,
            ShopName = "Đại Học FPT University",
            CustomerLat = order.CustomerLat,
            CustomerLng = order.CustomerLng,
            EstimatedDeliveryMinutes = order.EstimatedDeliveryMinutes,
            EstimatedDistanceMeters = order.EstimatedDistanceMeters,
            ShipperId = order.ShipperId,
            ShipperCurrentLat = order.ShipperCurrentLat,
            ShipperCurrentLng = order.ShipperCurrentLng,
            ShipperLocationUpdatedAt = order.ShipperLocationUpdatedAt,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            VoucherDiscount = order.VoucherDiscount,
            FinalAmount = order.FinalAmount,
            VoucherCode = order.VoucherCode,
            Note = order.Note,
            DeliveryFee = deliveryFee,
            PaymentMethod = paymentMethod,
            PaymentStatus = order.Payment?.Status,
            PaymentId = order.Payment?.Id,
            TransactionRef = order.Payment?.TransactionRef,
            PaymentUrl = paymentUrl,
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

    private static OrderTrackingResponse BuildTrackingResponse(Order order)
    {
        var statusInfo = GetStatusInfo(order.Status);
        var timeline = BuildStatusTimeline(order);

        return new OrderTrackingResponse
        {
            OrderId = order.Id,
            CurrentStatus = order.Status,
            StatusDisplayName = statusInfo.DisplayName,
            StatusDescription = statusInfo.Description,
            ShipperId = order.ShipperId,
            // Location data for map tracking
            ShopLat = order.ShopLat,
            ShopLng = order.ShopLng,
            CustomerLat = order.CustomerLat,
            CustomerLng = order.CustomerLng,
            ShipperCurrentLat = order.ShipperCurrentLat,
            ShipperCurrentLng = order.ShipperCurrentLng,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Timeline = timeline
        };
    }

    private static List<StatusTimelineItem> BuildStatusTimeline(Order order)
    {
        var timeline = new List<StatusTimelineItem>();
        var allStatuses = new[] { "pending", "confirmed", "shipping", "delivered" };
        var currentStatus = order.Status;

        foreach (var status in allStatuses)
        {
            var statusInfo = GetStatusInfo(status);
            var isCompleted = IsStatusCompleted(status, currentStatus);
            var isCurrent = status == currentStatus;

            // Xác định timestamp cho mỗi status
            DateTime timestamp;
            if (status == "pending")
            {
                timestamp = order.CreatedAt;
            }
            else if (isCompleted || isCurrent)
            {
                timestamp = order.UpdatedAt; // Sử dụng UpdatedAt cho status hiện tại hoặc đã hoàn thành
            }
            else
            {
                timestamp = order.CreatedAt; // Chưa đến status này
            }

            timeline.Add(new StatusTimelineItem
            {
                Status = status,
                StatusDisplayName = statusInfo.DisplayName,
                Description = statusInfo.Description,
                Timestamp = timestamp,
                IsCompleted = isCompleted,
                IsCurrent = isCurrent
            });
        }

        // Thêm cancelled nếu đơn bị hủy
        if (currentStatus == "cancelled")
        {
            timeline.Add(new StatusTimelineItem
            {
                Status = "cancelled",
                StatusDisplayName = "Đã hủy",
                Description = "Đơn hàng đã bị hủy",
                Timestamp = order.UpdatedAt,
                IsCompleted = true,
                IsCurrent = true
            });
        }

        return timeline;
    }

    private static bool IsStatusCompleted(string status, string currentStatus)
    {
        var statusOrder = new Dictionary<string, int>
        {
            { "pending", 1 },
            { "confirmed", 2 },
            { "shipping", 3 },
            { "delivered", 4 },
            { "cancelled", 0 }
        };

        if (currentStatus == "cancelled")
            return false; // Nếu bị hủy thì không có status nào completed

        if (!statusOrder.ContainsKey(status) || !statusOrder.ContainsKey(currentStatus))
            return false;

        return statusOrder[status] < statusOrder[currentStatus];
    }

    private static (string DisplayName, string Description) GetStatusInfo(string status)
    {
        return status switch
        {
            "pending" => ("Chờ xác nhận", "Đơn hàng đã được đặt, đang chờ shop xác nhận"),
            "confirmed" => ("Đã xác nhận", "Shop đã xác nhận đơn hàng, đang chờ shipper nhận đơn"),
            "shipping" => ("Đang giao hàng", "Shipper đang trên đường giao hàng đến bạn"),
            "delivered" => ("Đã giao hàng", "Đơn hàng đã được giao thành công"),
            "cancelled" => ("Đã hủy", "Đơn hàng đã bị hủy"),
            _ => (status, "Trạng thái không xác định")
        };
    }
}
