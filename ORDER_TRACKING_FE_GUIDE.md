# Hướng Dẫn FE: Trang Tracking Đơn Hàng

## Mục đích
Sau khi customer tạo order thành công, chuyển sang trang "Đơn hàng của tôi" để tracking đơn hàng theo thời gian thực.

## Luồng hoạt động

### 1. Sau khi tạo order thành công
- Sau khi gọi `POST /api/orders` thành công (status 201)
- Lấy `orderId` từ response
- Navigate đến trang tracking đơn hàng

### 2. Trang Tracking Đơn Hàng

#### API Endpoint
```
GET /api/orders/{id}/tracking
```

#### Response Structure
```typescript
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  statusCode: number;
}

interface OrderTrackingResponse {
  orderId: number;
  currentStatus: string; // "pending" | "confirmed" | "shipping" | "delivered" | "cancelled"
  statusDisplayName: string; // "Chờ xác nhận" | "Đã xác nhận" | "Đang giao hàng" | "Đã giao hàng"
  statusDescription: string; // Mô tả chi tiết
  shipperId: number | null; // ID shipper (null nếu chưa có shipper)
  createdAt: string; // ISO datetime
  updatedAt: string; // ISO datetime
  timeline: StatusTimelineItem[];
}

interface StatusTimelineItem {
  status: string;
  statusDisplayName: string;
  description: string;
  timestamp: string; // ISO datetime
  isCompleted: boolean; // Đã hoàn thành chưa
  isCurrent: boolean; // Có phải status hiện tại không
}
```

#### Example Response
```json
{
  "success": true,
  "data": {
    "orderId": 1,
    "currentStatus": "shipping",
    "statusDisplayName": "Đang giao hàng",
    "statusDescription": "Shipper đang trên đường giao hàng đến bạn",
    "shipperId": 5,
    "createdAt": "2026-02-27T14:00:00Z",
    "updatedAt": "2026-02-27T14:30:00Z",
    "timeline": [
      {
        "status": "pending",
        "statusDisplayName": "Chờ xác nhận",
        "description": "Đơn hàng đã được đặt, đang chờ shop xác nhận",
        "timestamp": "2026-02-27T14:00:00Z",
        "isCompleted": true,
        "isCurrent": false
      },
      {
        "status": "confirmed",
        "statusDisplayName": "Đã xác nhận",
        "description": "Shop đã xác nhận đơn hàng, đang chờ shipper nhận đơn",
        "timestamp": "2026-02-27T14:15:00Z",
        "isCompleted": true,
        "isCurrent": false
      },
      {
        "status": "shipping",
        "statusDisplayName": "Đang giao hàng",
        "description": "Shipper đang trên đường giao hàng đến bạn",
        "timestamp": "2026-02-27T14:30:00Z",
        "isCompleted": false,
        "isCurrent": true
      },
      {
        "status": "delivered",
        "statusDisplayName": "Đã giao hàng",
        "description": "Đơn hàng đã được giao thành công",
        "timestamp": "2026-02-27T14:00:00Z",
        "isCompleted": false,
        "isCurrent": false
      }
    ]
  },
  "message": "Order tracking retrieved successfully",
  "statusCode": 200
}
```

## UI/UX Gợi ý

### 1. Timeline Component
- Hiển thị timeline dọc với các bước:
  - ✅ **Pending** (Chờ xác nhận) - Màu xám
  - ✅ **Confirmed** (Đã xác nhận) - Màu xanh lá
  - 🔄 **Shipping** (Đang giao hàng) - Màu vàng/cam (nếu là current)
  - ⏳ **Delivered** (Đã giao hàng) - Màu xám (nếu chưa đến)

### 2. Status Badge
- Hiển thị status hiện tại với màu sắc phù hợp
- Hiển thị `statusDescription` để user biết đang ở bước nào

### 3. Shipper Info
- Nếu `shipperId` có giá trị → Hiển thị "Shipper #5 đang giao hàng"
- Nếu `shipperId` null → Hiển thị "Đang chờ shipper nhận đơn"

### 4. Auto Refresh
- Polling mỗi 10-15 giây để cập nhật status mới nhất
- Hoặc dùng WebSocket nếu có

### 5. Chi tiết đơn hàng
- Có thể gọi thêm `GET /api/orders/{id}` để lấy thông tin chi tiết (items, địa chỉ, giá, etc.)

## Error Handling

### 404 - Order not found
```json
{
  "success": false,
  "data": null,
  "message": "Order with id 1 not found",
  "statusCode": 404
}
```

### 400 - Bad Request
```json
{
  "success": false,
  "data": null,
  "message": "Error message",
  "statusCode": 400
}
```

## Code Example (React/TypeScript)

```typescript
// Fetch tracking data
const fetchOrderTracking = async (orderId: number) => {
  try {
    const response = await fetch(`/api/orders/${orderId}/tracking`);
    const data: ApiResponse<OrderTrackingResponse> = await response.json();
    
    if (data.success) {
      return data.data;
    } else {
      throw new Error(data.message);
    }
  } catch (error) {
    console.error('Error fetching order tracking:', error);
    throw error;
  }
};

// Component
const OrderTrackingPage = ({ orderId }: { orderId: number }) => {
  const [tracking, setTracking] = useState<OrderTrackingResponse | null>(null);
  
  useEffect(() => {
    // Fetch initial data
    fetchOrderTracking(orderId).then(setTracking);
    
    // Polling every 15 seconds
    const interval = setInterval(() => {
      fetchOrderTracking(orderId).then(setTracking);
    }, 15000);
    
    return () => clearInterval(interval);
  }, [orderId]);
  
  if (!tracking) return <Loading />;
  
  return (
    <div>
      <StatusBadge status={tracking.currentStatus} />
      <Timeline items={tracking.timeline} />
      {tracking.shipperId && (
        <ShipperInfo shipperId={tracking.shipperId} />
      )}
    </div>
  );
};
```

## Status Mapping

| Status | Display Name | Description | Color |
|--------|-------------|-------------|-------|
| `pending` | Chờ xác nhận | Đơn hàng đã được đặt, đang chờ shop xác nhận | Gray |
| `confirmed` | Đã xác nhận | Shop đã xác nhận đơn hàng, đang chờ shipper nhận đơn | Green |
| `shipping` | Đang giao hàng | Shipper đang trên đường giao hàng đến bạn | Orange/Yellow |
| `delivered` | Đã giao hàng | Đơn hàng đã được giao thành công | Blue |
| `cancelled` | Đã hủy | Đơn hàng đã bị hủy | Red |
