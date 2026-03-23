using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Orders.DTOs.Requests;

public class CreateOrderRequest
{
    public CustomerInfo Customer { get; set; } = new();
    public DeliveryAddressInfo DeliveryAddress { get; set; } = new();
    public List<OrderItemRequest> Items { get; set; } = new();
    public decimal TotalPrice { get; set; } // Tổng tiền trước giảm giá
    public string? VoucherCode { get; set; } // Mã voucher (optional)
    public string? Note { get; set; }
    public PaymentMethod? PaymentMethod { get; set; } // Default to COD if not specified
    public string? ClientIpAddress { get; set; } // Set by API endpoint for VNPay
}

public class CustomerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class DeliveryAddressInfo
{
    public string AddressDetail { get; set; } = string.Empty;
    public int? WardCode { get; set; }
    public int? DistrictCode { get; set; }
    public int? ProvinceCode { get; set; }
    public string? FullAddress { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
