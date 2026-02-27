# Hướng Dẫn FE: Shipper Update Status Đơn Hàng

## Mục đích
Trang dành cho shipper để xem danh sách đơn hàng và cập nhật status (nhận đơn → đang giao → đã giao).

## Luồng hoạt động

### 1. Shipper xem danh sách đơn hàng

#### API Endpoint
```
GET /api/orders?shipper_id={shipperId}&status={status}
```

#### Query Parameters
- `shipper_id` (required): ID của shipper
- `status` (optional): Lọc theo status ("confirmed", "shipping", "delivered")

#### Example Request
```
GET /api/orders?shipper_id=5&status=confirmed
```

#### Response Structure
```typescript
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  statusCode: number;
}

interface OrderResponse {
  id: number;
  customerName: string;
  customerPhone: string;
  fullAddress: string;
  shopLat: number;
  shopLng: number;
  shopName: string;
  customerLat: number | null;
  customerLng: number | null;
  estimatedDeliveryMinutes: number | null;
  estimatedDistanceMeters: number | null;
  shipperId: number | null;
  status: string;
  totalPrice: number | null;
  voucherDiscount: number | null;
  finalAmount: number | null;
  voucherCode: string | null;
  note: string | null;
  deliveryFee: number | null;
  paymentMethod: "COD" | null;
  createdAt: string;
  updatedAt: string;
  items: OrderItemResponse[];
}
```

### 2. Shipper nhận đơn (Chấp nhận đơn)

#### API Endpoint
```
PATCH /api/orders/{id}/shipper-status
```

#### Request Body
```typescript
interface UpdateShipperStatusRequest {
  shipperId: number;
  status: "shipping" | "delivered";
}
```

#### Example Request
```json
{
  "shipperId": 5,
  "status": "shipping"
}
```

#### Logic
- Khi shipper chấp nhận đơn (confirmed → shipping):
  - Nếu order chưa có `shipperId` → Tự động gán `shipperId` vào order
  - Nếu order đã có `shipperId` khác → Báo lỗi (đơn đã được gán cho shipper khác)
- Status phải là "confirmed" mới có thể chuyển sang "shipping"

#### Example Response (Success)
```json
{
  "success": true,
  "data": {
    "id": 1,
    "status": "shipping",
    "shipperId": 5,
    // ... other fields
  },
  "message": "Order status updated successfully by shipper",
  "statusCode": 200
}
```

#### Error Responses

**401 Unauthorized** - Shipper không có quyền
```json
{
  "success": false,
  "data": null,
  "message": "Unauthorized",
  "statusCode": 401
}
```

**400 Bad Request** - Status transition không hợp lệ
```json
{
  "success": false,
  "data": null,
  "message": "Invalid status transition from confirmed to delivered",
  "statusCode": 400
}
```

### 3. Shipper cập nhật "Đã giao hàng"

#### API Endpoint
```
PATCH /api/orders/{id}/shipper-status
```

#### Request Body
```json
{
  "shipperId": 5,
  "status": "delivered"
}
```

#### Logic
- Status phải là "shipping" mới có thể chuyển sang "delivered"
- Phải validate `shipperId` khớp với shipper được gán

## UI/UX Gợi ý

### 1. Danh sách đơn hàng cho shipper

#### Tab/Filter theo status:
- **Chờ nhận đơn** (`status=confirmed`): Đơn hàng đã được shop xác nhận, chưa có shipper
- **Đang giao** (`status=shipping`): Đơn hàng shipper đang giao
- **Đã giao** (`status=delivered`): Đơn hàng đã giao xong

#### Card hiển thị:
- Order ID
- Tên khách hàng
- Địa chỉ giao hàng
- Khoảng cách (nếu có)
- Thời gian dự kiến
- Tổng tiền
- Button "Nhận đơn" (nếu status = confirmed)
- Button "Đã giao hàng" (nếu status = shipping)

### 2. Button Actions

#### Button "Nhận đơn" (status = confirmed)
- Click → Gọi API `PATCH /api/orders/{id}/shipper-status` với `status: "shipping"`
- Show loading state
- Success → Cập nhật UI, chuyển sang tab "Đang giao"
- Error → Show error message

#### Button "Đã giao hàng" (status = shipping)
- Click → Confirm dialog "Xác nhận đã giao hàng?"
- Confirm → Gọi API `PATCH /api/orders/{id}/shipper-status` với `status: "delivered"`
- Success → Cập nhật UI, chuyển sang tab "Đã giao"

### 3. Map View (Optional)
- Nếu có `customerLat` và `customerLng`:
  - Hiển thị map với route từ shop đến customer
  - Có thể dùng OSRM để vẽ route

## Code Example (React/TypeScript)

```typescript
// Fetch orders for shipper
const fetchShipperOrders = async (shipperId: number, status?: string) => {
  const url = `/api/orders?shipper_id=${shipperId}${status ? `&status=${status}` : ''}`;
  const response = await fetch(url);
  const data: ApiResponse<OrderResponse[]> = await response.json();
  
  if (data.success) {
    return data.data;
  } else {
    throw new Error(data.message);
  }
};

// Update shipper status
const updateShipperStatus = async (
  orderId: number, 
  shipperId: number, 
  status: "shipping" | "delivered"
) => {
  const response = await fetch(`/api/orders/${orderId}/shipper-status`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      shipperId,
      status,
    }),
  });
  
  const data: ApiResponse<OrderResponse> = await response.json();
  
  if (data.success) {
    return data.data;
  } else {
    throw new Error(data.message);
  }
};

// Component
const ShipperOrdersPage = ({ shipperId }: { shipperId: number }) => {
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [selectedStatus, setSelectedStatus] = useState<string>('confirmed');
  
  useEffect(() => {
    fetchShipperOrders(shipperId, selectedStatus).then(setOrders);
  }, [shipperId, selectedStatus]);
  
  const handleAcceptOrder = async (orderId: number) => {
    try {
      await updateShipperStatus(orderId, shipperId, 'shipping');
      // Refresh orders
      fetchShipperOrders(shipperId, selectedStatus).then(setOrders);
    } catch (error) {
      alert('Lỗi: ' + error.message);
    }
  };
  
  const handleDeliverOrder = async (orderId: number) => {
    if (!confirm('Xác nhận đã giao hàng?')) return;
    
    try {
      await updateShipperStatus(orderId, shipperId, 'delivered');
      // Refresh orders
      fetchShipperOrders(shipperId, selectedStatus).then(setOrders);
    } catch (error) {
      alert('Lỗi: ' + error.message);
    }
  };
  
  return (
    <div>
      <Tabs>
        <Tab onClick={() => setSelectedStatus('confirmed')}>
          Chờ nhận đơn
        </Tab>
        <Tab onClick={() => setSelectedStatus('shipping')}>
          Đang giao
        </Tab>
        <Tab onClick={() => setSelectedStatus('delivered')}>
          Đã giao
        </Tab>
      </Tabs>
      
      {orders.map(order => (
        <OrderCard key={order.id} order={order}>
          {order.status === 'confirmed' && (
            <Button onClick={() => handleAcceptOrder(order.id)}>
              Nhận đơn
            </Button>
          )}
          {order.status === 'shipping' && (
            <Button onClick={() => handleDeliverOrder(order.id)}>
              Đã giao hàng
            </Button>
          )}
        </OrderCard>
      ))}
    </div>
  );
};
```

## Status Flow

```
confirmed (Đã xác nhận)
    ↓ [Shipper nhận đơn]
shipping (Đang giao hàng)
    ↓ [Shipper giao xong]
delivered (Đã giao hàng)
```

## Lưu ý

1. **Shipper ID**: Phải lấy từ authentication/session của shipper
2. **Validation**: Backend sẽ validate shipperId, không cần validate ở FE
3. **Auto Refresh**: Có thể polling để cập nhật danh sách đơn hàng mới
4. **Error Handling**: Luôn handle các error cases (401, 400, 404)
