# API CRUD Flow Guide - Food Booking System

Tài liệu này mô tả đầy đủ các flow CRUD từ request đến response cho Dashboard và Frontend.

## 📋 Mục lục

1. [Authentication & Authorization](#1-authentication--authorization)
2. [Categories](#2-categories)
3. [Brands](#3-brands)
4. [Products](#4-products)
5. [Orders](#5-orders)
6. [Vouchers](#6-vouchers)
7. [Payments](#7-payments)
8. [Locations](#8-locations)
9. [Common Response Format](#9-common-response-format)

---

## 1. Authentication & Authorization

### 1.1. Register (Đăng ký)

**Endpoint:** `POST /api/auth/register`

**Authentication:** Không cần

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "phoneNumber": "0123456789",  // Optional
  "fullName": "Nguyễn Văn A"     // Optional
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Registration successful. OTP sent to email. Please verify to activate account.",
  "data": true
}
```

**Response Error (400):**
```json
{
  "code": 400,
  "message": "Email already exists",
  "data": null
}
```

---

### 1.2. Verify OTP (Xác thực OTP và đăng nhập)

**Endpoint:** `POST /api/auth/verify-otp`

**Authentication:** Không cần

**Request Body:**
```json
{
  "email": "user@example.com",
  "otp": "123456"
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "OTP verified successfully. Account activated and logged in.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresAt": "2024-12-31T23:59:59Z",
    "user": {
      "userId": 1,
      "email": "user@example.com",
      "fullName": "Nguyễn Văn A",
      "userRole": "Customer"
    }
  }
}
```

**Response Error (401):**
```json
{
  "code": 401,
  "message": "Invalid OTP",
  "data": null
}
```

---

### 1.3. Login (Đăng nhập)

**Endpoint:** `POST /api/auth/login`

**Authentication:** Không cần

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresAt": "2024-12-31T23:59:59Z",
    "user": {
      "userId": 1,
      "email": "user@example.com",
      "fullName": "Nguyễn Văn A",
      "userRole": "Customer"
    }
  }
}
```

**Response Error (401):**
```json
{
  "code": 401,
  "message": "Invalid credentials",
  "data": null
}
```

---

### 1.4. Google Login

**Endpoint:** `POST /api/auth/google-login`

**Authentication:** Không cần

**Request Body:**
```json
{
  "idToken": "google_id_token_here"
}
```

**Response:** Tương tự như Login

---

### 1.5. Get Profile (Lấy thông tin profile)

**Endpoint:** `GET /api/auth/profile`

**Authentication:** Required (Bearer Token)

**Headers:**
```
Authorization: Bearer {token}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Profile retrieved successfully",
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0123456789",
    "userRole": "Customer",
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

**Response Error (401):**
```json
{
  "code": 401,
  "message": "Unauthorized",
  "data": null
}
```

---

### 1.6. Send OTP (Gửi lại OTP)

**Endpoint:** `POST /api/auth/send-otp`

**Authentication:** Không cần

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "OTP sent successfully",
  "data": true
}
```

---

## 2. Categories

### 2.1. Get All Categories (Lấy danh sách categories)

**Endpoint:** `GET /api/categories`

**Authentication:** Không cần

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 10, max: 100)
- `searchTerm` (string, optional)

**Example:** `GET /api/categories?pageNumber=1&pageSize=10&searchTerm=drink`

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Categories retrieved successfully",
  "data": {
    "data": [
      {
        "categoryId": 1,
        "name": "Đồ uống",
        "description": "Các loại đồ uống",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 10,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

---

### 2.2. Get Category By ID

**Endpoint:** `GET /api/categories/{id}`

**Authentication:** Không cần

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Category retrieved successfully",
  "data": {
    "categoryId": 1,
    "name": "Đồ uống",
    "description": "Các loại đồ uống",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Response Error (404):**
```json
{
  "code": 404,
  "message": "Category not found",
  "data": null
}
```

---

### 2.3. Create Category

**Endpoint:** `POST /api/categories`

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "name": "Đồ uống",
  "description": "Các loại đồ uống"  // Optional
}
```

**Response Success (201):**
```json
{
  "code": 200,
  "message": "Category created successfully",
  "data": {
    "categoryId": 1,
    "name": "Đồ uống",
    "description": "Các loại đồ uống",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Response Error (400):**
```json
{
  "code": 400,
  "message": "Category name already exists",
  "data": null
}
```

---

### 2.4. Update Category

**Endpoint:** `PUT /api/categories/{id}`

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "name": "Đồ uống mới",
  "description": "Mô tả mới"  // Optional
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Category updated successfully",
  "data": {
    "categoryId": 1,
    "name": "Đồ uống mới",
    "description": "Mô tả mới",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-02T00:00:00Z"
  }
}
```

**Response Error (404):**
```json
{
  "code": 404,
  "message": "Category with id 1 not found",
  "data": null
}
```

---

### 2.5. Delete Category

**Endpoint:** `DELETE /api/categories/{id}`

**Authentication:** Required (Bearer Token)

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Category deleted successfully",
  "data": null
}
```

**Response Error (404):**
```json
{
  "code": 404,
  "message": "Category with id 1 not found",
  "data": null
}
```

---

## 3. Brands

### 3.1. Get All Brands

**Endpoint:** `GET /api/brands`

**Authentication:** Không cần

**Query Parameters:** Tương tự Categories (pageNumber, pageSize, searchTerm)

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Brands retrieved successfully",
  "data": {
    "data": [
      {
        "brandId": 1,
        "name": "Coca Cola",
        "description": "Thương hiệu nước ngọt",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

---

### 3.2. Get Brand By ID

**Endpoint:** `GET /api/brands/{id}`

**Authentication:** Không cần

**Response:** Tương tự Category

---

### 3.3. Create Brand

**Endpoint:** `POST /api/brands`

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "name": "Coca Cola",
  "description": "Thương hiệu nước ngọt"  // Optional
}
```

**Response Success (201):**
```json
{
  "code": 200,
  "message": "Brand created successfully",
  "data": {
    "brandId": 1,
    "name": "Coca Cola",
    "description": "Thương hiệu nước ngọt",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### 3.4. Update Brand

**Endpoint:** `PUT /api/brands/{id}`

**Authentication:** Required (Bearer Token)

**Request Body:** Tương tự Create

**Response:** Tương tự Create (200)

---

### 3.5. Delete Brand

**Endpoint:** `DELETE /api/brands/{id}`

**Authentication:** Required (Bearer Token)

**Response:** Tương tự Delete Category

---

## 4. Products

### 4.1. Get All Products

**Endpoint:** `GET /api/products`

**Authentication:** Không cần

**Query Parameters:** pageNumber, pageSize, searchTerm

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Products retrieved successfully",
  "data": {
    "data": [
      {
        "productId": 1,
        "name": "Coca Cola 330ml",
        "description": "Nước ngọt có ga",
        "price": 15000,
        "stockQuantity": 100,
        "categoryId": 1,
        "categoryName": "Đồ uống",
        "brandId": 1,
        "brandName": "Coca Cola",
        "isActive": true,
        "productSizes": [
          {
            "productSizeId": 1,
            "size": "330ml",
            "price": 15000,
            "stockQuantity": 100,
            "isActive": true
          }
        ],
        "images": [
          {
            "productImageId": 1,
            "imageUrl": "https://cloudinary.com/image.jpg",
            "isPrimary": true,
            "sortOrder": 1
          }
        ],
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

### 4.2. Get Product By ID

**Endpoint:** `GET /api/products/{id}`

**Authentication:** Không cần

**Response:** Tương tự Get All (nhưng chỉ 1 item trong data)

---

### 4.3. Get Products By Category

**Endpoint:** `GET /api/products/category/{categoryId}`

**Authentication:** Không cần

**Query Parameters:** pageNumber, pageSize, searchTerm

**Response:** Tương tự Get All Products

---

### 4.4. Get Products By Brand

**Endpoint:** `GET /api/products/brand/{brandId}`

**Authentication:** Không cần

**Query Parameters:** pageNumber, pageSize, searchTerm

**Response:** Tương tự Get All Products

---

### 4.5. Create Product

**Endpoint:** `POST /api/products`

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "name": "Coca Cola 330ml",
  "description": "Nước ngọt có ga",
  "categoryId": 1,
  "brandId": 1,
  "isActive": true,
  "imageUrls": [
    "https://cloudinary.com/image1.jpg",
    "https://cloudinary.com/image2.jpg"
  ],
  "productSizes": [
    {
      "size": "330ml",
      "price": 15000,
      "stockQuantity": 100
    },
    {
      "size": "500ml",
      "price": 20000,
      "stockQuantity": 50
    }
  ]
}
```

**Response Success (201):**
```json
{
  "code": 200,
  "message": "Product created successfully",
  "data": {
    "productId": 1,
    "name": "Coca Cola 330ml",
    "description": "Nước ngọt có ga",
    "price": 15000,
    "stockQuantity": 100,
    "categoryId": 1,
    "categoryName": "Đồ uống",
    "brandId": 1,
    "brandName": "Coca Cola",
    "isActive": true,
    "productSizes": [...],
    "images": [...],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Response Error (404):**
```json
{
  "code": 404,
  "message": "Category with id 1 not found",
  "data": null
}
```

---

### 4.6. Update Product

**Endpoint:** `PUT /api/products/{id}`

**Authentication:** Required (Bearer Token)

**Request Body:** Tương tự Create Product

**Response:** Tương tự Create (200)

---

### 4.7. Delete Product

**Endpoint:** `DELETE /api/products/{id}`

**Authentication:** Required (Bearer Token)

**Response:** Tương tự Delete Category

---

## 5. Orders

### 5.1. Estimate Delivery (Tính thời gian giao hàng)

**Endpoint:** `POST /api/orders/estimate-delivery`

**Authentication:** Không cần

**Request Body:**
```json
{
  "customerLat": 10.841449,
  "customerLng": 106.809997
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Delivery time estimated successfully",
  "data": {
    "estimatedMinutes": 30,
    "estimatedDistanceMeters": 5000,
    "deliveryFee": 20000
  }
}
```

---

### 5.2. Create Order

**Endpoint:** `POST /api/orders`

**Authentication:** Không cần

**Request Body:**
```json
{
  "customer": {
    "name": "Nguyễn Văn A",
    "phone": "0123456789"
  },
  "deliveryAddress": {
    "addressDetail": "123 Đường ABC",
    "wardCode": 12345,
    "districtCode": 678,
    "provinceCode": 79,
    "fullAddress": "123 Đường ABC, Phường 1, Quận 1, TP.HCM",
    "lat": 10.841449,
    "lng": 106.809997
  },
  "items": [
    {
      "productId": 1,
      "productName": "Coca Cola 330ml",
      "quantity": 2,
      "unitPrice": 15000,
      "subtotal": 30000
    }
  ],
  "totalPrice": 30000,
  "voucherCode": "DISCOUNT10",  // Optional
  "note": "Giao hàng nhanh",     // Optional
  "paymentMethod": "COD"          // Optional, default: COD
}
```

**Response Success (201):**
```json
{
  "code": 200,
  "message": "Order created successfully",
  "data": {
    "id": 1,
    "customerName": "Nguyễn Văn A",
    "customerPhone": "0123456789",
    "fullAddress": "123 Đường ABC, Phường 1, Quận 1, TP.HCM",
    "shopLat": 10.841449,
    "shopLng": 106.809997,
    "shopName": "Đại Học FPT University",
    "customerLat": 10.841449,
    "customerLng": 106.809997,
    "estimatedDeliveryMinutes": 30,
    "estimatedDistanceMeters": 5000,
    "shipperId": null,
    "shipperCurrentLat": null,
    "shipperCurrentLng": null,
    "shipperLocationUpdatedAt": null,
    "status": "pending",
    "totalPrice": 30000,
    "voucherDiscount": 3000,
    "finalAmount": 27000,
    "voucherCode": "DISCOUNT10",
    "note": "Giao hàng nhanh",
    "deliveryFee": 20000,
    "paymentMethod": "COD",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z",
    "items": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Coca Cola 330ml",
        "unitPrice": 15000,
        "quantity": 2,
        "subtotal": 30000
      }
    ]
  }
}
```

---

### 5.3. Get Order By ID

**Endpoint:** `GET /api/orders/{id}`

**Authentication:** Không cần

**Response:** Tương tự Create Order (200)

---

### 5.4. Get All Orders (Admin)

**Endpoint:** `GET /api/orders`

**Authentication:** Required (Bearer Token)

**Response Success (200):**
```json
{
  "code": 200,
  "message": "All orders retrieved successfully",
  "data": [
    {
      "id": 1,
      "customerName": "Nguyễn Văn A",
      "customerPhone": "0123456789",
      "status": "pending",
      "totalPrice": 30000,
      "finalAmount": 27000,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z",
      "items": [...]
    }
  ]
}
```

---

### 5.5. Get My Orders (Customer)

**Endpoint:** `GET /api/orders/my-orders`

**Authentication:** Required (Bearer Token)

**Response:** Tương tự Get All Orders

---

### 5.6. Get Order Tracking

**Endpoint:** `GET /api/orders/{id}/tracking`

**Authentication:** Không cần

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Order tracking retrieved successfully",
  "data": {
    "orderId": 1,
    "currentStatus": "shipping",
    "statusDisplayName": "Đang giao hàng",
    "statusDescription": "Shipper đang trên đường đến",
    "shipperId": 2,
    "shopLat": 10.841449,
    "shopLng": 106.809997,
    "customerLat": 10.850000,
    "customerLng": 106.810000,
    "shipperCurrentLat": 10.845000,
    "shipperCurrentLng": 106.809500,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T01:00:00Z",
    "timeline": [
      {
        "status": "pending",
        "statusDisplayName": "Chờ xử lý",
        "timestamp": "2024-01-01T00:00:00Z"
      },
      {
        "status": "shipping",
        "statusDisplayName": "Đang giao hàng",
        "timestamp": "2024-01-01T01:00:00Z"
      }
    ]
  }
}
```

---

### 5.7. Update Order Status

**Endpoint:** `PATCH /api/orders/{id}/status`

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "status": "confirmed"  // "pending", "confirmed", "shipping", "delivered", "cancelled"
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Order status updated successfully",
  "data": {
    "id": 1,
    "status": "confirmed",
    ...
  }
}
```

---

### 5.8. Assign Shipper

**Endpoint:** `PATCH /api/orders/{id}/assign-shipper`

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "shipperId": 2
}
```

**Response:** Tương tự Update Order Status

---

### 5.9. Update Shipper Status (Shipper nhận đơn)

**Endpoint:** `PATCH /api/orders/{id}/shipper-status`

**Authentication:** Required (Bearer Token - Shipper)

**Request Body:**
```json
{
  "shipperId": 2,
  "status": "shipping",  // "shipping" hoặc "delivered"
  "lat": 10.841449,      // Optional, required khi nhận đơn
  "lng": 106.809997      // Optional, required khi nhận đơn
}
```

**Response:** Tương tự Update Order Status

**Note:** Khi shipper nhận đơn (status = "shipping"), SignalR sẽ tự động notify customer.

---

### 5.10. Update Shipper Location (Realtime tracking)

**Endpoint:** `POST /api/orders/{id}/shipper-location`

**Authentication:** Required (Bearer Token - Shipper)

**Request Body:**
```json
{
  "lat": 10.841449,
  "lng": 106.809997
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Shipper location updated successfully",
  "data": null
}
```

**Note:** Location được broadcast qua SignalR đến customer đang track order.

---

### 5.11. Get Shipper Orders

**Endpoint:** `GET /api/orders/shipper/my-orders`

**Authentication:** Required (Bearer Token - Shipper)

**Query Parameters:**
- `status` (string, optional): Filter theo status

**Example:** `GET /api/orders/shipper/my-orders?status=shipping`

**Response:** Tương tự Get All Orders

---

### 5.12. Get Available Orders (Shipper)

**Endpoint:** `GET /api/orders/shipper/available`

**Authentication:** Required (Bearer Token - Shipper)

**Response:** Tương tự Get All Orders (chỉ orders có status = "pending" hoặc "confirmed" và chưa có shipper)

---

## 6. Vouchers

### 6.1. Get Active Vouchers

**Endpoint:** `GET /api/vouchers`

**Authentication:** Không cần

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Vouchers retrieved successfully",
  "data": [
    {
      "id": 1,
      "code": "DISCOUNT10",
      "name": "Giảm 10%",
      "description": "Giảm 10% cho đơn hàng trên 100k",
      "discountType": "percentage",
      "discountValue": 10,
      "minOrderAmount": 100000,
      "maxDiscountAmount": 50000,
      "usageLimit": 100,
      "usedCount": 25,
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-12-31T23:59:59Z",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

---

### 6.2. Get Voucher By Code

**Endpoint:** `GET /api/vouchers/{code}`

**Authentication:** Không cần

**Response:** Tương tự Get Active Vouchers (nhưng chỉ 1 item)

---

### 6.3. Validate Voucher

**Endpoint:** `POST /api/vouchers/validate`

**Authentication:** Không cần

**Request Body:**
```json
{
  "code": "DISCOUNT10",
  "orderAmount": 200000
}
```

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Voucher is valid",
  "data": {
    "id": 1,
    "code": "DISCOUNT10",
    "name": "Giảm 10%",
    "discountType": "percentage",
    "discountValue": 10,
    "minOrderAmount": 100000,
    "maxDiscountAmount": 50000,
    ...
  }
}
```

**Response Error (400):**
```json
{
  "code": 400,
  "message": "Voucher has expired",
  "data": null
}
```

---

### 6.4. Create Voucher

**Endpoint:** `POST /api/vouchers`

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "code": "DISCOUNT10",
  "name": "Giảm 10%",
  "description": "Giảm 10% cho đơn hàng trên 100k",
  "discountType": "percentage",  // "percentage" | "fixed_amount"
  "discountValue": 10,
  "minOrderAmount": 100000,      // Optional
  "maxDiscountAmount": 50000,    // Optional
  "usageLimit": 100,             // Optional
  "startDate": "2024-01-01T00:00:00Z",  // Optional
  "endDate": "2024-12-31T23:59:59Z",    // Optional
  "isActive": true
}
```

**Response Success (201):**
```json
{
  "code": 200,
  "message": "Voucher created successfully",
  "data": {
    "id": 1,
    "code": "DISCOUNT10",
    "name": "Giảm 10%",
    ...
  }
}
```

---

## 7. Payments

### 7.1. Create Payment

**Endpoint:** `POST /api/payments`

**Authentication:** Không cần

**Request Body:**
```json
{
  "orderId": 1,
  "paymentMethod": "COD"  // "COD" | "BANK_TRANSFER" | "MOMO" | "VNPAY"
}
```

**Response Success (201):**
```json
{
  "code": 200,
  "message": "Payment created successfully",
  "data": {
    "id": 1,
    "orderId": 1,
    "paymentMethod": "COD",
    "paymentMethodName": "Thanh toán khi nhận hàng",
    "status": "Pending",
    "statusName": "Chờ thanh toán",
    "amount": 50000,
    "transactionRef": null,
    "paymentMetadata": null,
    "paidAt": null,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### 7.2. Get Payment By Order ID

**Endpoint:** `GET /api/payments/order/{orderId}`

**Authentication:** Không cần

**Response:** Tương tự Create Payment

---

### 7.3. Update Payment Status

**Endpoint:** `PATCH /api/payments/{id}/status`

**Authentication:** Không cần

**Request Body:**
```json
{
  "status": "Completed"  // "Pending" | "Completed" | "Failed" | "Refunded"
}
```

**Response:** Tương tự Create Payment (200)

---

## 8. Locations

### 8.1. Get All Provinces

**Endpoint:** `GET /api/locations/provinces`

**Authentication:** Không cần

**Response Success (200):**
```json
{
  "code": 200,
  "message": "Provinces retrieved successfully",
  "data": [
    {
      "code": 79,
      "name": "Thành phố Hồ Chí Minh",
      "nameEn": "Ho Chi Minh City",
      "fullName": "Thành phố Hồ Chí Minh",
      "fullNameEn": "Ho Chi Minh City",
      "codeName": "ho-chi-minh",
      "latitude": 10.8231,
      "longitude": 106.6297
    }
  ]
}
```

---

### 8.2. Get Districts By Province

**Endpoint:** `GET /api/locations/districts?province_code=79`

**Authentication:** Không cần

**Response:** Tương tự Provinces

---

### 8.3. Get Wards By District

**Endpoint:** `GET /api/locations/wards?district_code=760`

**Authentication:** Không cần

**Response:** Tương tự Provinces

---

### 8.4. Update Province Coordinates

**Endpoint:** `PUT /api/locations/provinces/{code}/coordinates`

**Authentication:** Không cần

**Request Body:**
```json
{
  "latitude": 10.8231,
  "longitude": 106.6297
}
```

**Response:** Tương tự Get All Provinces (200)

---

## 9. Common Response Format

### Success Response

Tất cả API đều trả về format chuẩn:

```json
{
  "code": 200,
  "message": "Success message",
  "data": { ... }  // Hoặc array, hoặc null
}
```

### Error Response

```json
{
  "code": 400,  // 400, 401, 403, 404, 500
  "message": "Error message",
  "data": null,
  "errors": {  // Optional, cho validation errors
    "email": ["Email is required"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

### Paginated Response

```json
{
  "code": 200,
  "message": "Success",
  "data": {
    "data": [...],           // Array of items
    "totalCount": 100,       // Total items
    "pageNumber": 1,         // Current page
    "pageSize": 10,           // Items per page
    "totalPages": 10,         // Total pages
    "hasPreviousPage": false, // Has previous page
    "hasNextPage": true       // Has next page
  }
}
```

---

## 10. Authentication Header Format

Tất cả API yêu cầu authentication đều cần header:

```
Authorization: Bearer {token}
```

**Lưu ý:**
- Token có thể gửi với hoặc không có prefix "Bearer "
- Token được lấy từ response của Login/Verify OTP/Google Login
- Token có thời hạn (expiresAt trong AuthResponse)

---

## 11. Status Codes

- **200 OK**: Request thành công
- **201 Created**: Tạo mới thành công
- **400 Bad Request**: Request không hợp lệ
- **401 Unauthorized**: Chưa đăng nhập hoặc token không hợp lệ
- **403 Forbidden**: Không có quyền truy cập
- **404 Not Found**: Không tìm thấy resource
- **500 Internal Server Error**: Lỗi server

---

## 12. Order Status Flow

1. **pending** → Chờ xử lý (mới tạo)
2. **confirmed** → Đã xác nhận
3. **shipping** → Đang giao hàng (shipper đã nhận)
4. **delivered** → Đã giao hàng
5. **cancelled** → Đã hủy

---

## 13. Payment Status

- **Pending**: Chờ thanh toán
- **Completed**: Đã thanh toán
- **Failed**: Thanh toán thất bại
- **Refunded**: Đã hoàn tiền

---

## 14. Payment Methods

- **COD**: Thanh toán khi nhận hàng
- **BANK_TRANSFER**: Chuyển khoản ngân hàng
- **MOMO**: Ví điện tử MoMo
- **VNPAY**: Ví điện tử VNPay

---

## 15. Notes cho Dashboard

### Dashboard nên hiển thị:

1. **Statistics:**
   - Tổng số đơn hàng (theo status)
   - Tổng doanh thu
   - Số lượng sản phẩm
   - Số lượng khách hàng

2. **Orders Management:**
   - Danh sách đơn hàng với filter (status, date range)
   - Chi tiết đơn hàng
   - Cập nhật status đơn hàng
   - Gán shipper

3. **Products Management:**
   - CRUD Products
   - CRUD Categories
   - CRUD Brands
   - Upload images

4. **Vouchers Management:**
   - CRUD Vouchers
   - Xem usage statistics

5. **Payments:**
   - Xem danh sách payments
   - Cập nhật payment status

---

## 16. Example Flow: Tạo đơn hàng hoàn chỉnh

1. **Get Products:** `GET /api/products` → Chọn sản phẩm
2. **Get Locations:** `GET /api/locations/provinces` → Chọn địa chỉ
3. **Estimate Delivery:** `POST /api/orders/estimate-delivery` → Tính phí ship
4. **Validate Voucher (optional):** `POST /api/vouchers/validate` → Validate voucher
5. **Create Order:** `POST /api/orders` → Tạo đơn hàng
6. **Create Payment:** `POST /api/payments` → Tạo payment
7. **Get Order Tracking:** `GET /api/orders/{id}/tracking` → Track đơn hàng

---

**Chúc bạn làm dashboard thành công! 🚀**
