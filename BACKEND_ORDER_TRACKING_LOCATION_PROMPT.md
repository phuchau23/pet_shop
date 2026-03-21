# Backend API: Order Tracking Location Data Implementation

## Vấn đề hiện tại

API `/api/orders/{id}/tracking` hiện tại **KHÔNG trả về location data** (tất cả đều `null`):
- `shopLat`, `shopLng` - Vị trí cửa hàng
- `customerLat`, `customerLng` - Vị trí khách hàng (địa chỉ giao hàng)
- `shipperCurrentLat`, `shipperCurrentLng` - Vị trí hiện tại của shipper (realtime)

## Yêu cầu

Cần **bổ sung location data** vào response của API `/api/orders/{id}/tracking` để frontend có thể hiển thị map tracking đơn hàng.

## Response Format mong muốn

API `/api/orders/{id}/tracking` cần trả về JSON với format sau:

```json
{
  "code": 200,
  "message": "Success",
  "data": {
    "orderId": 6,
    "currentStatus": "shipping",
    "statusDisplayName": "Đang giao hàng",
    "statusDescription": "Đơn hàng đang được giao đến bạn",
    "shipperId": 3,
    "shipperCurrentLat": 21.0285,        // ⭐ CẦN THÊM
    "shipperCurrentLng": 105.8542,        // ⭐ CẦN THÊM
    "shopLat": 21.0285,                   // ⭐ CẦN THÊM
    "shopLng": 105.8542,                  // ⭐ CẦN THÊM
    "customerLat": 21.0300,               // ⭐ CẦN THÊM
    "customerLng": 105.8600,             // ⭐ CẦN THÊM
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T11:00:00Z",
    "timeline": [...]
  }
}
```

## Logic cần implement

### 1. **Shop Location** (`shopLat`, `shopLng`)
- Lấy từ **địa chỉ cửa hàng** (shop address) trong database
- Có thể hardcode một địa chỉ cửa hàng mặc định hoặc lấy từ bảng `Shop`/`Store`
- **Bắt buộc phải có** (không được null) vì đây là điểm xuất phát của shipper

### 2. **Customer Location** (`customerLat`, `customerLng`)
- Lấy từ **địa chỉ giao hàng** (delivery address) trong order
- Parse từ field `deliveryAddress` hoặc `address` trong bảng `Order`
- Nếu địa chỉ là text, cần **geocode** (convert địa chỉ → lat/lng) bằng Google Maps Geocoding API hoặc service tương tự
- **Bắt buộc phải có** (không được null) vì đây là điểm đích đến

### 3. **Shipper Current Location** (`shipperCurrentLat`, `shipperCurrentLng`)
- Lấy từ **location tracking data** của shipper cho order này
- Có thể lấy từ:
  - Bảng `ShipperLocation` hoặc `LocationTracking` (nếu có)
  - Hoặc từ SignalR hub nếu đang tracking realtime
  - Hoặc từ bảng `Order` nếu có field `shipperCurrentLat`, `shipperCurrentLng`
- **Có thể null** nếu shipper chưa bắt đầu di chuyển hoặc chưa có location data

## Implementation Steps

### Step 1: Xác định nguồn dữ liệu
1. **Shop Location**: 
   - Kiểm tra xem có bảng `Shop`/`Store` không?
   - Nếu có, lấy `latitude`, `longitude` từ đó
   - Nếu không, hardcode một địa chỉ cửa hàng mặc định (ví dụ: Hà Nội)

2. **Customer Location**:
   - Kiểm tra bảng `Order` có field `deliveryAddress` không?
   - Nếu có địa chỉ dạng text, cần geocode sang lat/lng
   - Nếu đã có `deliveryLat`, `deliveryLng` trong Order, dùng luôn

3. **Shipper Location**:
   - Kiểm tra xem có bảng tracking location không?
   - Nếu có, lấy location mới nhất của shipper cho order này
   - Nếu không, có thể lấy từ SignalR hub state hoặc để null

### Step 2: Update DTO/Model
Thêm các field sau vào DTO response của Order Tracking:
```csharp
public class OrderTrackingResponseDto
{
    public int OrderId { get; set; }
    public string CurrentStatus { get; set; }
    public string StatusDisplayName { get; set; }
    public string StatusDescription { get; set; }
    public int? ShipperId { get; set; }
    
    // ⭐ THÊM CÁC FIELD NÀY
    public double? ShopLat { get; set; }
    public double? ShopLng { get; set; }
    public double? CustomerLat { get; set; }
    public double? CustomerLng { get; set; }
    public double? ShipperCurrentLat { get; set; }
    public double? ShipperCurrentLng { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<StatusTimelineItemDto> Timeline { get; set; }
}
```

### Step 3: Update Service/Repository
Trong service xử lý Order Tracking, thêm logic để populate location data:

```csharp
public async Task<OrderTrackingResponseDto> GetOrderTracking(int orderId)
{
    var order = await _orderRepository.GetByIdAsync(orderId);
    if (order == null) throw new NotFoundException("Order not found");
    
    var response = new OrderTrackingResponseDto
    {
        OrderId = order.Id,
        CurrentStatus = order.Status,
        // ... other fields ...
        
        // ⭐ THÊM LOCATION DATA
        // 1. Shop Location
        ShopLat = await GetShopLatitude(),
        ShopLng = await GetShopLongitude(),
        
        // 2. Customer Location
        CustomerLat = await GetCustomerLatitude(order.DeliveryAddress),
        CustomerLng = await GetCustomerLongitude(order.DeliveryAddress),
        
        // 3. Shipper Location (nếu có)
        ShipperCurrentLat = await GetShipperCurrentLatitude(orderId),
        ShipperCurrentLng = await GetShipperCurrentLongitude(orderId),
    };
    
    return response;
}
```

### Step 4: Implement helper methods

#### GetShopLocation()
```csharp
private async Task<double> GetShopLatitude()
{
    // Option 1: Lấy từ bảng Shop
    var shop = await _shopRepository.GetDefaultShopAsync();
    if (shop?.Latitude != null) return shop.Latitude.Value;
    
    // Option 2: Hardcode (Hà Nội)
    return 21.0285;
}

private async Task<double> GetShopLongitude()
{
    var shop = await _shopRepository.GetDefaultShopAsync();
    if (shop?.Longitude != null) return shop.Longitude.Value;
    return 105.8542;
}
```

#### GetCustomerLocation()
```csharp
private async Task<double?> GetCustomerLatitude(string address)
{
    // Option 1: Nếu Order đã có lat/lng
    if (order.DeliveryLat != null) return order.DeliveryLat;
    
    // Option 2: Geocode từ địa chỉ text
    if (!string.IsNullOrEmpty(address))
    {
        var geocodeResult = await _geocodingService.GeocodeAsync(address);
        return geocodeResult?.Latitude;
    }
    
    return null;
}
```

#### GetShipperLocation()
```csharp
private async Task<double?> GetShipperCurrentLatitude(int orderId)
{
    // Lấy location mới nhất của shipper cho order này
    var latestLocation = await _locationTrackingRepository
        .GetLatestByOrderIdAsync(orderId);
    
    return latestLocation?.Latitude;
}
```

## Testing

Sau khi implement, test API với order có status = "shipping":

```bash
GET /api/orders/6/tracking
Authorization: Bearer {token}
```

**Expected Response:**
- `shopLat`, `shopLng` phải có giá trị (không null)
- `customerLat`, `customerLng` phải có giá trị (không null)
- `shipperCurrentLat`, `shipperCurrentLng` có thể null nếu shipper chưa di chuyển

## Notes

1. **Geocoding**: Nếu cần geocode địa chỉ text → lat/lng, có thể dùng:
   - Google Maps Geocoding API
   - OpenStreetMap Nominatim (free)
   - Hoặc lưu lat/lng ngay khi customer nhập địa chỉ

2. **Performance**: Nếu geocoding tốn thời gian, có thể:
   - Cache kết quả geocoding
   - Lưu lat/lng vào database khi tạo order
   - Async geocoding khi order được tạo

3. **Fallback**: Nếu không có location data, có thể:
   - Trả về location mặc định (ví dụ: trung tâm thành phố)
   - Hoặc trả về null và frontend sẽ hiển thị placeholder

## Priority

**HIGH** - Frontend cần location data để hiển thị map tracking. Hiện tại map không hiển thị được vì tất cả location đều null.
