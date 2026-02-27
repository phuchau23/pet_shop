# Hướng Dẫn FE: Get Profile và My Orders API

## Tổng quan

API đã được cập nhật để sử dụng JWT token để xác định user, **không cần truyền tham số** như `customer_phone` nữa. Tất cả thông tin user được lấy từ token.

## ⚠️ Lưu ý quan trọng

**API cũ `GET /api/orders?customer_phone=...` đã được thay thế:**
- ❌ **KHÔNG DÙNG**: `GET /api/orders?customer_phone=...` (API cũ)
- ✅ **DÙNG MỚI**: `GET /api/orders/my-orders` (API mới - tự động lấy từ token)

---

## 1. API Get Profile

### Endpoint
```
GET /api/auth/profile
```

### Authentication
- **Required**: JWT Token trong Authorization header
- Format: `Authorization: Bearer {token}`

### Mô tả
Lấy thông tin profile của user hiện tại từ token. Không cần truyền bất kỳ tham số nào.

### Request Example
```http
GET /api/auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response Structure
```typescript
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  statusCode: number;
}

interface ProfileResponse {
  userId: number;
  email: string;
  fullName: string | null;
  phoneNumber: string | null;
  userRole: string; // "Customer" | "Admin"
  accountStatus: string; // "Active" | "Inactive" | "Banned"
  lastLogin: string | null; // ISO datetime
  createdAt: string; // ISO datetime
  updatedAt: string; // ISO datetime
}
```

### Example Response
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0901234567",
    "userRole": "Customer",
    "accountStatus": "Active",
    "lastLogin": "2026-02-27T14:00:00Z",
    "createdAt": "2026-01-01T00:00:00Z",
    "updatedAt": "2026-02-27T14:00:00Z"
  },
  "message": "Profile retrieved successfully",
  "statusCode": 200
}
```

### Error Responses

#### 401 Unauthorized - Không có token hoặc token không hợp lệ
```json
{
  "success": false,
  "data": null,
  "message": "Unauthorized",
  "statusCode": 401
}
```

#### 404 Not Found - User không tồn tại
```json
{
  "success": false,
  "data": null,
  "message": "User with id 1 not found",
  "statusCode": 404
}
```

---

## 2. API Get My Orders

### Endpoint
```
GET /api/orders/my-orders
```

### Authentication
- **Required**: JWT Token trong Authorization header
- Format: `Authorization: Bearer {token}`
- **Không cần truyền `customer_phone`** - Tự động lấy từ profile

### Mô tả
Lấy danh sách đơn hàng của user hiện tại. API sẽ:
1. Lấy `userId` từ token
2. Gọi GetProfile để lấy `phoneNumber`
3. Lấy orders theo `phoneNumber`

### Request Example
```http
GET /api/orders/my-orders
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response Structure
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
  fullAddress: string | null;
  shopLat: number;
  shopLng: number;
  shopName: string;
  customerLat: number | null;
  customerLng: number | null;
  estimatedDeliveryMinutes: number | null;
  estimatedDistanceMeters: number | null;
  shipperId: number | null;
  status: string; // "pending" | "confirmed" | "shipping" | "delivered" | "cancelled"
  totalPrice: number | null;
  voucherDiscount: number | null;
  finalAmount: number | null;
  voucherCode: string | null;
  note: string | null;
  deliveryFee: number | null;
  paymentMethod: "COD" | null;
  createdAt: string; // ISO datetime
  updatedAt: string; // ISO datetime
  items: OrderItemResponse[];
}

interface OrderItemResponse {
  id: number;
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
}
```

### Example Response
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "customerName": "Nguyễn Văn A",
      "customerPhone": "0901234567",
      "fullAddress": "123 Nguyễn Huệ, Quận 1, TP.HCM",
      "shopLat": 10.841449,
      "shopLng": 106.809997,
      "shopName": "Đại Học FPT University",
      "customerLat": 10.7769,
      "customerLng": 106.7009,
      "estimatedDeliveryMinutes": 30,
      "estimatedDistanceMeters": 15000,
      "shipperId": 5,
      "status": "shipping",
      "totalPrice": 150000,
      "voucherDiscount": 0,
      "finalAmount": 150000,
      "voucherCode": null,
      "note": "Giao giờ hành chính",
      "deliveryFee": 20000,
      "paymentMethod": "COD",
      "createdAt": "2026-02-27T14:00:00Z",
      "updatedAt": "2026-02-27T14:30:00Z",
      "items": [
        {
          "id": 1,
          "productId": 10,
          "productName": "Bánh mì thịt nướng",
          "unitPrice": 50000,
          "quantity": 3,
          "subtotal": 150000
        }
      ]
    }
  ],
  "message": "My orders retrieved successfully",
  "statusCode": 200
}
```

### Error Responses

#### 401 Unauthorized - Không có token hoặc token không hợp lệ
```json
{
  "success": false,
  "data": null,
  "message": "Unauthorized",
  "statusCode": 401
}
```

#### 400 Bad Request - User chưa có phone number
```json
{
  "success": false,
  "data": null,
  "message": "Phone number not found in profile. Please update your profile.",
  "statusCode": 400
}
```

#### 404 Not Found - User không tồn tại
```json
{
  "success": false,
  "data": null,
  "message": "User with id 1 not found",
  "statusCode": 404
}
```

---

## 3. Migration từ API cũ sang API mới

### ❌ API CŨ (KHÔNG DÙNG NỮA)
```typescript
// ❌ KHÔNG DÙNG
const response = await fetch(`/api/orders?customer_phone=${phoneNumber}`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

### ✅ API MỚI (DÙNG)
```typescript
// ✅ DÙNG
const response = await fetch('/api/orders/my-orders', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

---

## 4. Code Example (React/TypeScript)

### Get Profile
```typescript
const getProfile = async (token: string) => {
  try {
    const response = await fetch('/api/auth/profile', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    const data: ApiResponse<ProfileResponse> = await response.json();
    
    if (data.success) {
      return data.data;
    } else {
      throw new Error(data.message);
    }
  } catch (error) {
    console.error('Error fetching profile:', error);
    throw error;
  }
};

// Usage
const ProfilePage = () => {
  const [profile, setProfile] = useState<ProfileResponse | null>(null);
  const token = localStorage.getItem('token'); // hoặc từ context/state

  useEffect(() => {
    if (token) {
      getProfile(token).then(setProfile);
    }
  }, [token]);

  if (!profile) return <Loading />;

  return (
    <div>
      <h1>Profile</h1>
      <p>Email: {profile.email}</p>
      <p>Full Name: {profile.fullName}</p>
      <p>Phone: {profile.phoneNumber}</p>
      <p>Role: {profile.userRole}</p>
    </div>
  );
};
```

### Get My Orders
```typescript
const getMyOrders = async (token: string) => {
  try {
    const response = await fetch('/api/orders/my-orders', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    const data: ApiResponse<OrderResponse[]> = await response.json();
    
    if (data.success) {
      return data.data;
    } else {
      throw new Error(data.message);
    }
  } catch (error) {
    console.error('Error fetching my orders:', error);
    throw error;
  }
};

// Usage
const MyOrdersPage = () => {
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const token = localStorage.getItem('token');

  useEffect(() => {
    if (token) {
      getMyOrders(token)
        .then(setOrders)
        .catch(console.error)
        .finally(() => setLoading(false));
    }
  }, [token]);

  if (loading) return <Loading />;

  return (
    <div>
      <h1>My Orders</h1>
      {orders.length === 0 ? (
        <p>No orders found</p>
      ) : (
        orders.map(order => (
          <OrderCard key={order.id} order={order} />
        ))
      )}
    </div>
  );
};
```

### Combined Hook (Recommended)
```typescript
import { useState, useEffect } from 'react';

const useUserData = (token: string | null) => {
  const [profile, setProfile] = useState<ProfileResponse | null>(null);
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) {
      setLoading(false);
      return;
    }

    const fetchData = async () => {
      try {
        setLoading(true);
        
        // Fetch profile and orders in parallel
        const [profileData, ordersData] = await Promise.all([
          getProfile(token),
          getMyOrders(token)
        ]);

        setProfile(profileData);
        setOrders(ordersData);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [token]);

  return { profile, orders, loading, error };
};

// Usage
const MyAccountPage = () => {
  const token = localStorage.getItem('token');
  const { profile, orders, loading, error } = useUserData(token);

  if (loading) return <Loading />;
  if (error) return <Error message={error} />;

  return (
    <div>
      <ProfileSection profile={profile} />
      <MyOrdersSection orders={orders} />
    </div>
  );
};
```

---

## 5. Lưu ý quan trọng

### 1. Token phải được gửi trong mọi request
- Tất cả API đều yêu cầu JWT token
- Format: `Authorization: Bearer {token}`
- Token được lấy từ login/register response

### 2. Phone Number là bắt buộc cho My Orders
- User phải có `phoneNumber` trong profile để lấy orders
- Nếu chưa có, cần update profile trước
- API sẽ trả về error 400 nếu thiếu phone number

### 3. Error Handling
- Luôn check `data.success` trước khi sử dụng `data.data`
- Handle các error cases: 401 (Unauthorized), 400 (Bad Request), 404 (Not Found)

### 4. Token Storage
- Lưu token ở nơi an toàn (localStorage, sessionStorage, hoặc secure cookie)
- Không hardcode token trong code
- Refresh token khi hết hạn

---

## 6. So sánh API cũ vs mới

| Feature | API Cũ | API Mới |
|---------|--------|---------|
| Endpoint | `GET /api/orders?customer_phone=...` | `GET /api/orders/my-orders` |
| Tham số | Cần truyền `customer_phone` | Không cần tham số |
| Authentication | Optional | Required (JWT Token) |
| Security | Có thể truy cập orders của user khác | Chỉ lấy orders của chính user |
| User Experience | Phải lưu phone number | Tự động từ token |

---

## 7. Checklist Migration

- [ ] Thay thế tất cả calls từ `GET /api/orders?customer_phone=...` sang `GET /api/orders/my-orders`
- [ ] Đảm bảo JWT token được gửi trong Authorization header
- [ ] Thêm error handling cho trường hợp user chưa có phone number
- [ ] Update UI để hiển thị thông báo khi cần update profile
- [ ] Test với các trường hợp: có token, không có token, token hết hạn
- [ ] Test với user chưa có phone number

---

## 8. Xử lý Nullable Fields (Quan trọng!)

### ⚠️ Lỗi thường gặp: "type 'Null' is not a subtype of type 'int'"

Lỗi này xảy ra khi parse các field nullable thành non-nullable int. Các field sau có thể null:

#### Trong OrderResponse:
- `shipperId`: `int | null` - Có thể null nếu chưa có shipper
- `estimatedDeliveryMinutes`: `int | null` - Có thể null
- `paymentMethod`: `"COD" | null` - Có thể null

#### Cách xử lý trong Dart/Flutter:

```dart
// ❌ SAI - Sẽ lỗi nếu null
int shipperId = order['shipperId'] as int; // Lỗi nếu null!

// ✅ ĐÚNG - Handle null
int? shipperId = order['shipperId'] as int?;
// hoặc
int shipperId = order['shipperId'] ?? 0; // Default value
// hoặc
int? shipperId = order['shipperId'] != null ? order['shipperId'] as int : null;
```

#### Model Class Example (Dart):

```dart
class OrderResponse {
  final int id;
  final String customerName;
  final String customerPhone;
  final String? fullAddress;
  final int? shipperId; // ⚠️ Nullable
  final int? estimatedDeliveryMinutes; // ⚠️ Nullable
  final String status;
  final double? totalPrice; // ⚠️ Nullable
  final double? deliveryFee; // ⚠️ Nullable
  final String? paymentMethod; // ⚠️ Nullable
  final List<OrderItemResponse> items;

  OrderResponse({
    required this.id,
    required this.customerName,
    required this.customerPhone,
    this.fullAddress,
    this.shipperId, // Nullable
    this.estimatedDeliveryMinutes, // Nullable
    required this.status,
    this.totalPrice, // Nullable
    this.deliveryFee, // Nullable
    this.paymentMethod, // Nullable
    required this.items,
  });

  factory OrderResponse.fromJson(Map<String, dynamic> json) {
    return OrderResponse(
      id: json['id'] as int,
      customerName: json['customerName'] as String,
      customerPhone: json['customerPhone'] as String,
      fullAddress: json['fullAddress'] as String?,
      shipperId: json['shipperId'] as int?, // ⚠️ Nullable
      estimatedDeliveryMinutes: json['estimatedDeliveryMinutes'] as int?, // ⚠️ Nullable
      status: json['status'] as String,
      totalPrice: json['totalPrice'] != null 
        ? (json['totalPrice'] as num).toDouble() 
        : null, // ⚠️ Handle null
      deliveryFee: json['deliveryFee'] != null 
        ? (json['deliveryFee'] as num).toDouble() 
        : null, // ⚠️ Handle null
      paymentMethod: json['paymentMethod'] as String?,
      items: (json['items'] as List)
        .map((item) => OrderItemResponse.fromJson(item))
        .toList(),
    );
  }
}
```

#### Safe Parsing Helper:

```dart
// Helper function để parse nullable int an toàn
int? safeParseInt(dynamic value) {
  if (value == null) return null;
  if (value is int) return value;
  if (value is String) return int.tryParse(value);
  return null;
}

// Usage
int? shipperId = safeParseInt(json['shipperId']);
int? estimatedMinutes = safeParseInt(json['estimatedDeliveryMinutes']);
```

#### Khi sử dụng trong UI:

```dart
// ✅ Safe - Check null trước khi dùng
if (order.shipperId != null) {
  Text('Shipper ID: ${order.shipperId}');
} else {
  Text('Chưa có shipper');
}

// ✅ Safe - Dùng null-aware operator
Text('Shipper: ${order.shipperId ?? "N/A"}');
Text('Thời gian: ${order.estimatedDeliveryMinutes ?? 0} phút');
```

### ⚠️ Lỗi khi đăng xuất

Khi đăng xuất, đảm bảo:
1. Clear token trước khi clear user data
2. Không gọi API sau khi đã clear token
3. Handle null khi parse response

```dart
void logout() {
  // 1. Clear token trước
  await storage.delete('token');
  
  // 2. Clear user data
  userProvider.clearUser();
  
  // 3. Navigate to login
  Navigator.pushReplacementNamed(context, '/login');
}
```

---

## 9. Support

Nếu gặp vấn đề, kiểm tra:
1. Token có hợp lệ không?
2. Token có được gửi đúng format không?
3. User có phone number trong profile không?
4. Network request có thành công không? (check status code)
5. **Có handle nullable fields đúng cách không?** ⚠️
6. **Có clear token trước khi clear user data không?** ⚠️
