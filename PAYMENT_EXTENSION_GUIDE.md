# Hướng dẫn mở rộng Payment Gateway

## 📋 Cấu trúc hiện tại

Hệ thống thanh toán hiện tại chỉ hỗ trợ **COD (Cash on Delivery)** nhưng đã được thiết kế để dễ dàng mở rộng thêm các payment gateway khác.

## 🔧 Cách thêm Payment Gateway mới

### Bước 1: Thêm PaymentMethod vào enum

Cập nhật `src/FoodBooking.Domain/Enums/PaymentMethod.cs`:

```csharp
public enum PaymentMethod
{
    COD = 1,
    ZaloPay = 2,  // Thêm mới
    MoMo = 3,     // Thêm mới
    VNPay = 4,    // Thêm mới
    // etc.
}
```

### Bước 2: Tạo Payment Gateway Service

Tạo interface trong `src/FoodBooking.Application/Abstractions/`:

```csharp
// IZaloPayService.cs (ví dụ)
public interface IZaloPayService
{
    Task<PaymentGatewayResponse?> CreatePaymentAsync(string orderId, decimal amount, string description, CancellationToken cancellationToken = default);
    Task<bool> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default);
}
```

Tạo implementation trong `src/FoodBooking.Infrastructure/External/ZaloPay/`:

```csharp
// ZaloPayService.cs
public class ZaloPayService : IZaloPayService
{
    // Implementation...
}
```

### Bước 3: Cập nhật PaymentService

Trong `PaymentService.CreatePaymentAsync()`, thêm case mới:

```csharp
switch (request.PaymentMethod)
{
    case PaymentMethod.COD:
        // COD logic
        break;
    
    case PaymentMethod.ZaloPay:  // Thêm mới
        var zaloPayService = // Get from DI
        var response = await zaloPayService.CreatePaymentAsync(...);
        
        // Lưu metadata vào PaymentMetadata (JSON)
        payment.PaymentMetadata = JsonSerializer.Serialize(new
        {
            qrCodeUrl = response.QrCodeUrl,
            paymentUrl = response.PaymentUrl,
            gatewayOrderId = response.OrderId
        });
        break;
    
    // Thêm các gateway khác...
}
```

### Bước 4: Cập nhật PaymentMethodName

Trong `PaymentService.MapToResponse()`, thêm case mới:

```csharp
PaymentMethodName = payment.PaymentMethod switch
{
    PaymentMethod.COD => "COD",
    PaymentMethod.ZaloPay => "ZaloPay",  // Thêm mới
    PaymentMethod.MoMo => "MoMo",        // Thêm mới
    _ => payment.PaymentMethod.ToString()
}
```

### Bước 5: Đăng ký Service trong Program.cs

```csharp
// Register Payment Gateway Service
builder.Services.AddHttpClient<IZaloPayService, ZaloPayService>();
```

### Bước 6: Thêm Callback Endpoint (nếu cần)

Trong `PaymentEndpoints.cs`, thêm endpoint callback:

```csharp
// POST /payments/zalopay/callback
group.MapPost("/zalopay/callback", async (...) =>
{
    // Handle callback từ ZaloPay
});
```

### Bước 7: Cấu hình trong appsettings.json

```json
{
  "ZaloPay": {
    "AppId": "...",
    "Key1": "...",
    "Key2": "...",
    "ApiEndpoint": "...",
    "CallbackUrl": "..."
  }
}
```

## 📝 PaymentMetadata Field

Field `PaymentMetadata` là JSON string để lưu thông tin từ payment gateway:

```json
{
  "qrCodeUrl": "https://...",
  "paymentUrl": "https://...",
  "gatewayOrderId": "...",
  "deepLink": "...",
  // Bất kỳ thông tin nào khác từ gateway
}
```

Khi cần lấy thông tin, parse JSON:

```csharp
var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.PaymentMetadata);
var qrCodeUrl = metadata?["qrCodeUrl"]?.ToString();
```

## 🎯 Ví dụ: Thêm ZaloPay

1. Thêm `ZaloPay = 2` vào `PaymentMethod` enum
2. Tạo `IZaloPayService` và `ZaloPayService`
3. Trong `PaymentService`, thêm case `PaymentMethod.ZaloPay`
4. Lưu QR code URL, payment URL vào `PaymentMetadata` (JSON)
5. Đăng ký service trong `Program.cs`
6. Thêm callback endpoint nếu cần
7. Cấu hình credentials trong `appsettings.json`

## ✅ Lợi ích của thiết kế này

- **Dễ mở rộng**: Chỉ cần thêm enum value và case mới
- **Linh hoạt**: `PaymentMetadata` có thể lưu bất kỳ thông tin nào từ gateway
- **Không cần migration**: Không cần thêm cột mới cho mỗi gateway
- **Tách biệt**: Mỗi gateway có service riêng, dễ maintain
