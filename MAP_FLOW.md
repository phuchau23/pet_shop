Backend Specification – Delivery App
Thông tin chung
DatabasePostgreSQLMapOpenStreetMap (Flutter gọi thẳng, BE không xử lý)RoutingOSRM (Flutter gọi thẳng, BE không xử lý)Vị trí shoplat: 10.8506, lng: 106.7749Địa chỉ shop7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, TP.HCM 700000ShipperNhân viên nội bộ của shop, không phải bên thứ 3

Cấu trúc Database
Bảng provinces
sqlCREATE TABLE provinces (
    id            SERIAL PRIMARY KEY,
    code          INT UNIQUE NOT NULL,
    name          VARCHAR(100) NOT NULL,
    codename      VARCHAR(100),
    division_type VARCHAR(50),
    phone_code    INT,
    latitude      DOUBLE PRECISION,
    longitude     DOUBLE PRECISION
);
Bảng districts
sqlCREATE TABLE districts (
    id            SERIAL PRIMARY KEY,
    code          INT UNIQUE NOT NULL,
    name          VARCHAR(100) NOT NULL,
    codename      VARCHAR(100),
    division_type VARCHAR(50),
    province_code INT REFERENCES provinces(code)
);
Bảng wards
sqlCREATE TABLE wards (
    id            SERIAL PRIMARY KEY,
    code          INT UNIQUE NOT NULL,
    name          VARCHAR(100) NOT NULL,
    codename      VARCHAR(100),
    division_type VARCHAR(50),
    district_code INT REFERENCES districts(code)
);
Bảng orders
sqlCREATE TABLE orders (
    id               SERIAL PRIMARY KEY,
    customer_id      INT NOT NULL,
    shipper_id       INT,

    -- Địa chỉ giao hàng
    address_detail   VARCHAR(255) NOT NULL,   -- số nhà, tên đường
    ward_code        INT REFERENCES wards(code),
    district_code    INT REFERENCES districts(code),
    province_code    INT REFERENCES provinces(code),
    full_address     VARCHAR(500),             -- ghép đầy đủ để hiển thị
    customer_lat     DOUBLE PRECISION,         -- tọa độ điểm giao
    customer_lng     DOUBLE PRECISION,

    -- Điểm xuất phát (luôn là shop)
    shop_lat         DOUBLE PRECISION DEFAULT 10.8506,
    shop_lng         DOUBLE PRECISION DEFAULT 106.7749,

    -- Trạng thái
    status           VARCHAR(50) DEFAULT 'pending',
    -- pending | confirmed | shipping | delivered | cancelled

    total_price      DECIMAL(12,2),
    note             TEXT,
    created_at       TIMESTAMP DEFAULT NOW(),
    updated_at       TIMESTAMP DEFAULT NOW()
);

Seed Data
Chạy script Node.js một lần duy nhất để import 63 tỉnh + quận/huyện + phường/xã vào DB.
Source: https://provinces.open-api.vn/api/?depth=3
Script : seed.js (xem file riêng)

Sau khi seed xong, BE không cần gọi lại API provinces.open-api.vn nữa.


API Endpoints
1. Locations
GET /locations/provinces
Trả về danh sách 63 tỉnh thành.
Response:
json[
  {
    "code": 79,
    "name": "Thành phố Hồ Chí Minh",
    "latitude": 10.8231,
    "longitude": 106.6297
  }
]

GET /locations/districts?province_code={code}
Trả về danh sách quận/huyện theo tỉnh.
Query params:
ParamBắt buộcMô tảprovince_code✅Mã tỉnh
Response:
json[
  { "code": 760, "name": "Quận 1" },
  { "code": 761, "name": "Quận 2" }
]

GET /locations/wards?district_code={code}
Trả về danh sách phường/xã theo quận.
Query params:
ParamBắt buộcMô tảdistrict_code✅Mã quận/huyện
Response:
json[
  { "code": 26734, "name": "Phường Bến Nghé" }
]

2. Orders
POST /orders
Tạo đơn hàng mới. Flutter gửi địa chỉ đã chọn + tọa độ khách.
Request body:
json{
  "customer_id": 1,
  "address_detail": "123 Nguyễn Huệ",
  "ward_code": 26734,
  "district_code": 760,
  "province_code": 79,
  "full_address": "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
  "customer_lat": 10.7769,
  "customer_lng": 106.7009,
  "total_price": 150000,
  "note": "Giao giờ hành chính"
}
Response: 201 Created
json{
  "id": 101,
  "status": "pending",
  "shop_lat": 10.8506,
  "shop_lng": 106.7749,
  "customer_lat": 10.7769,
  "customer_lng": 106.7009,
  "full_address": "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
  "created_at": "2026-02-25T08:00:00Z"
}

GET /orders/{id}
Lấy chi tiết 1 đơn hàng. Flutter dùng để lấy tọa độ hiển thị map.
Response:
json{
  "id": 101,
  "status": "shipping",
  "full_address": "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
  "shop_lat": 10.8506,
  "shop_lng": 106.7749,
  "customer_lat": 10.7769,
  "customer_lng": 106.7009,
  "shipper_id": 5
}

GET /orders?customer_id={id}
Lấy danh sách đơn hàng của khách.

GET /orders?shipper_id={id}&status=confirmed
Lấy danh sách đơn hàng được giao cho shipper.

PATCH /orders/{id}/status
Cập nhật trạng thái đơn hàng (dùng cho cả shop và shipper).
Request body:
json{
  "status": "delivered"
}
Các giá trị status hợp lệ:
StatusMô tảpendingVừa đặt, chờ shop xác nhậnconfirmedShop đã xác nhận, chờ shipper nhậnshippingShipper đang giaodeliveredĐã giao thành côngcancelledĐã huỷ

PATCH /orders/{id}/assign-shipper
Shop gán shipper cho đơn hàng.
Request body:
json{
  "shipper_id": 5
}

Luồng nghiệp vụ
[Khách đặt hàng]
  POST /orders  →  status: pending

[Shop xác nhận]
  PATCH /orders/{id}/status  →  status: confirmed
  PATCH /orders/{id}/assign-shipper  →  gán shipper

[Shipper nhận đơn]
  GET /orders?shipper_id=5&status=confirmed
  PATCH /orders/{id}/status  →  status: shipping

  Flutter dùng shop_lat/shop_lng + customer_lat/customer_lng
  để vẽ route trên map (gọi OSRM trực tiếp, không qua BE)

[Giao hàng xong]
  PATCH /orders/{id}/status  →  status: delivered

Lưu ý quan trọng

BE không xử lý bản đồ hay route, Flutter tự gọi OSM và OSRM
BE chỉ lưu và trả về lat/lng để Flutter dùng
shop_lat và shop_lng luôn mặc định là 10.8506 và 106.7749
Shipper là nhân viên nội bộ, không phải bên thứ 3
Dữ liệu tỉnh/huyện/xã chỉ cần seed 1 lần duy nhất