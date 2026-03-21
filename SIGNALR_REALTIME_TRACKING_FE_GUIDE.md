# Hướng Dẫn FE: Realtime Tracking với SignalR

## Tổng Quan

Hệ thống sử dụng **SignalR** để tracking realtime vị trí shipper. Khi shipper nhận đơn và bắt đầu giao hàng, cả **shipper app** và **customer app** đều có thể xem vị trí realtime của shipper trên map.

## Flow Hoàn Chỉnh

```
1. Customer đặt hàng → Order status = "pending"
   ↓
2. Shipper xem danh sách available orders
   ↓
3. Shipper nhận đơn → PATCH /orders/{id}/shipper-status với status = "shipping"
   → Order status = "shipping", shipperId được gán
   → SignalR notify customer: "ShipperAssigned"
   ↓
4. Shipper app bắt đầu tracking GPS
   → Gửi location lên server mỗi 5-10 giây: POST /orders/{id}/shipper-location
   → Server broadcast qua SignalR: "ShipperLocationUpdated"
   ↓
5. Customer app nhận location updates → Update marker trên map
   ↓
6. Shipper đến nơi → Confirm delivered
```

## 1. SignalR Connection

### Endpoint
```
ws://localhost:5000/hubs/location?access_token={JWT_TOKEN}
```

### Authentication
- SignalR sử dụng JWT token từ query string `access_token`
- Token phải là token hợp lệ từ login API
- Không cần prefix "Bearer"

### Connect (Flutter/Dart)

```dart
import 'package:signalr_netcore/signalr_client.dart';

class SignalRService {
  late HubConnection _connection;
  String? _token;

  Future<void> connect(String token) async {
    _token = token;
    
    final hubUrl = 'http://localhost:5000/hubs/location';
    final options = HttpConnectionOptions(
      accessTokenFactory: () => Future.value(token),
    );
    
    _connection = HubConnectionBuilder()
        .withUrl(hubUrl, options: options)
        .withAutomaticReconnect()
        .build();

    await _connection.start();
    print('SignalR Connected');
  }

  Future<void> disconnect() async {
    await _connection.stop();
  }
}
```

## 2. Customer App - Tracking Order

### Bước 1: Connect SignalR và Join Order Group

```dart
// Sau khi customer đặt hàng thành công
Future<void> startTrackingOrder(int orderId) async {
  // Connect SignalR
  await signalRService.connect(userToken);
  
  // Join group để nhận updates cho order này
  await signalRService.connection.invoke(
    'JoinOrderTracking',
    args: [orderId],
  );
  
  // Listen for events
  signalRService.connection.on('ShipperAssigned', (message) {
    // Shipper đã nhận đơn
    final data = jsonDecode(message[0]);
    print('Shipper ${data['shipperId']} đã nhận đơn');
    // Update UI: Hiển thị thông tin shipper
  });
  
  signalRService.connection.on('ShipperLocationUpdated', (message) {
    // Vị trí shipper đã được update
    final data = jsonDecode(message[0]);
    final lat = data['lat'] as double;
    final lng = data['lng'] as double;
    
    // Update marker trên map
    updateShipperMarker(lat, lng);
  });
  
  signalRService.connection.on('OrderStatusChanged', (message) {
    // Status đơn hàng thay đổi
    final data = jsonDecode(message[0]);
    print('Order status: ${data['status']}');
    // Update UI
  });
}
```

### Bước 2: Hiển Thị Map với Shipper Location

```dart
// Khi nhận ShipperLocationUpdated event
void updateShipperMarker(double lat, double lng) {
  // Update marker trên map
  setState(() {
    shipperLat = lat;
    shipperLng = lng;
  });
  
  // Vẽ route từ shop → shipper → customer (nếu cần)
  drawRoute();
}
```

### Bước 3: Lấy Thông Tin Shipper

```dart
// GET /api/orders/{id}
// Response sẽ có:
{
  "id": 4,
  "shipperId": 3,
  "shipperCurrentLat": 10.8510,
  "shipperCurrentLng": 106.7850,
  "shipperLocationUpdatedAt": "2026-03-06T15:00:00Z",
  "status": "shipping",
  ...
}
```

## 3. Shipper App - Gửi Location

### Bước 1: Nhận Đơn và Bắt Đầu Tracking

```dart
// Shipper nhận đơn
Future<void> acceptOrder(int orderId) async {
  // PATCH /orders/{orderId}/shipper-status
  final response = await http.patch(
    Uri.parse('http://localhost:5000/api/orders/$orderId/shipper-status'),
    headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },
    body: jsonEncode({
      'shipperId': currentShipperId,
      'status': 'shipping',
    }),
  );
  
  if (response.statusCode == 200) {
    // Bắt đầu tracking GPS
    startLocationTracking(orderId);
    
    // Navigate đến map screen với route từ shop → customer
    navigateToMapScreen(orderId);
  }
}
```

### Bước 2: Tracking GPS và Gửi Location

```dart
import 'package:geolocator/geolocator.dart';

StreamSubscription<Position>? _positionStream;

Future<void> startLocationTracking(int orderId) async {
  // Request permission
  bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
  if (!serviceEnabled) {
    // Show error
    return;
  }
  
  LocationPermission permission = await Geolocator.checkPermission();
  if (permission == LocationPermission.denied) {
    permission = await Geolocator.requestPermission();
    if (permission == LocationPermission.denied) {
      return;
    }
  }
  
  // Listen to position changes
  _positionStream = Geolocator.getPositionStream(
    locationSettings: LocationSettings(
      accuracy: LocationAccuracy.high,
      distanceFilter: 10, // Update mỗi 10 mét
    ),
  ).listen((Position position) {
    // Gửi location lên server
    sendLocationToServer(orderId, position.latitude, position.longitude);
  });
}

Future<void> sendLocationToServer(int orderId, double lat, double lng) async {
  try {
    // POST /api/orders/{id}/shipper-location
    final response = await http.post(
      Uri.parse('http://localhost:5000/api/orders/$orderId/shipper-location'),
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
      body: jsonEncode({
        'lat': lat,
        'lng': lng,
      }),
    );
    
    if (response.statusCode == 200) {
      print('Location updated: $lat, $lng');
    }
  } catch (e) {
    print('Error sending location: $e');
  }
}

// Stop tracking khi đến nơi
void stopLocationTracking() {
  _positionStream?.cancel();
  _positionStream = null;
}
```

### Bước 3: Hiển Thị Route trên Map

```dart
// Khi vào map screen, vẽ route từ shop → customer
Future<void> drawRoute(int orderId) async {
  // Lấy order details
  final order = await getOrderDetails(orderId);
  
  // Vẽ route từ shop → customer
  // Sử dụng OSRM hoặc Google Maps Directions API
  final route = await getRoute(
    startLat: order.shopLat,
    startLng: order.shopLng,
    endLat: order.customerLat!,
    endLng: order.customerLng!,
  );
  
  // Hiển thị route trên map
  drawPolyline(route);
  
  // Hiển thị markers:
  // - Shop marker (start point)
  // - Customer marker (end point)
  // - Shipper marker (current position - update realtime)
}
```

## 4. API Endpoints

### POST /api/orders/{id}/shipper-location
**Mục đích:** Shipper gửi location lên server

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "lat": 10.851038263383241,
  "lng": 106.78499515268862
}
```

**Response:**
```json
{
  "code": 200,
  "message": "Shipper location updated successfully",
  "data": null
}
```

**Lưu ý:**
- Gọi endpoint này mỗi 5-10 giây (hoặc khi di chuyển > 10 mét)
- Server sẽ tự động broadcast qua SignalR đến customer

### GET /api/orders/{id}
**Mục đích:** Lấy thông tin order, bao gồm shipper location hiện tại

**Response:**
```json
{
  "code": 200,
  "data": {
    "id": 4,
    "shipperId": 3,
    "shipperCurrentLat": 10.8510,
    "shipperCurrentLng": 106.7850,
    "shipperLocationUpdatedAt": "2026-03-06T15:00:00Z",
    "status": "shipping",
    "shopLat": 10.841449,
    "shopLng": 106.809997,
    "customerLat": 10.851038263383241,
    "customerLng": 106.78499515268862,
    ...
  }
}
```

## 5. SignalR Events

### Events Customer Nhận Được

#### 1. `ShipperAssigned`
**Khi nào:** Khi shipper nhận đơn (status chuyển sang "shipping")

**Data:**
```json
{
  "orderId": 4,
  "shipperId": 3,
  "shipperName": "Nguyễn Văn A",
  "timestamp": "2026-03-06T15:00:00Z"
}
```

**Action:**
- Hiển thị thông báo: "Shipper đã nhận đơn"
- Bắt đầu listen cho location updates

#### 2. `ShipperLocationUpdated`
**Khi nào:** Mỗi khi shipper gửi location mới

**Data:**
```json
{
  "orderId": 4,
  "lat": 10.8510,
  "lng": 106.7850,
  "timestamp": "2026-03-06T15:00:05Z"
}
```

**Action:**
- Update marker shipper trên map
- Có thể vẽ route từ shipper → customer (nếu cần)

#### 3. `OrderStatusChanged`
**Khi nào:** Khi order status thay đổi

**Data:**
```json
{
  "orderId": 4,
  "status": "delivered",
  "statusDisplayName": "Đã giao hàng",
  "timestamp": "2026-03-06T15:30:00Z"
}
```

**Action:**
- Update UI với status mới
- Nếu "delivered" → Stop tracking

## 6. Best Practices

### Customer App
1. **Connect SignalR ngay sau khi đặt hàng thành công**
2. **Join order group** để nhận updates
3. **Listen cho 3 events:** `ShipperAssigned`, `ShipperLocationUpdated`, `OrderStatusChanged`
4. **Update map marker** mỗi khi nhận `ShipperLocationUpdated`
5. **Disconnect SignalR** khi:
   - Order status = "delivered" hoặc "cancelled"
   - User navigate away từ tracking screen

### Shipper App
1. **Bắt đầu tracking GPS** ngay sau khi nhận đơn
2. **Gửi location mỗi 5-10 giây** (hoặc khi di chuyển > 10 mét)
3. **Stop tracking** khi:
   - Đến nơi và confirm delivered
   - Order bị cancelled
4. **Hiển thị route** từ shop → customer để hướng dẫn đi

### Performance
- **Không gửi location quá thường xuyên** (tối đa 1 lần/giây)
- **Sử dụng distance filter** trong GPS để chỉ update khi di chuyển đáng kể
- **Disconnect SignalR** khi không cần thiết để tiết kiệm battery

## 7. Error Handling

### SignalR Connection Failed
```dart
_connection.onclose((error) {
  print('SignalR disconnected: $error');
  // Retry connection
  reconnect();
});
```

### Location Permission Denied
```dart
if (permission == LocationPermission.deniedForever) {
  // Show dialog hướng dẫn user enable location trong settings
  showLocationPermissionDialog();
}
```

### API Error
```dart
try {
  await sendLocationToServer(orderId, lat, lng);
} catch (e) {
  // Retry sau 5 giây
  Future.delayed(Duration(seconds: 5), () {
    sendLocationToServer(orderId, lat, lng);
  });
}
```

## 8. Example Code (Flutter)

### Full Example: Customer Tracking Screen

```dart
class OrderTrackingScreen extends StatefulWidget {
  final int orderId;
  
  const OrderTrackingScreen({required this.orderId});
  
  @override
  _OrderTrackingScreenState createState() => _OrderTrackingScreenState();
}

class _OrderTrackingScreenState extends State<OrderTrackingScreen> {
  late HubConnection _connection;
  double? shipperLat;
  double? shipperLng;
  int? shipperId;
  String status = 'pending';
  
  @override
  void initState() {
    super.initState();
    _connectSignalR();
  }
  
  Future<void> _connectSignalR() async {
    final token = await getToken();
    final hubUrl = 'http://localhost:5000/hubs/location';
    
    _connection = HubConnectionBuilder()
        .withUrl(hubUrl, HttpConnectionOptions(
          accessTokenFactory: () => Future.value(token),
        ))
        .withAutomaticReconnect()
        .build();
    
    // Listen for events
    _connection.on('ShipperAssigned', (message) {
      final data = jsonDecode(message[0]);
      setState(() {
        shipperId = data['shipperId'];
      });
    });
    
    _connection.on('ShipperLocationUpdated', (message) {
      final data = jsonDecode(message[0]);
      setState(() {
        shipperLat = data['lat'];
        shipperLng = data['lng'];
      });
    });
    
    _connection.on('OrderStatusChanged', (message) {
      final data = jsonDecode(message[0]);
      setState(() {
        status = data['status'];
      });
    });
    
    await _connection.start();
    await _connection.invoke('JoinOrderTracking', args: [widget.orderId]);
  }
  
  @override
  void dispose() {
    _connection.stop();
    super.dispose();
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Tracking Order #${widget.orderId}')),
      body: GoogleMap(
        initialCameraPosition: CameraPosition(
          target: LatLng(10.841449, 106.809997),
          zoom: 14,
        ),
        markers: {
          if (shipperLat != null && shipperLng != null)
            Marker(
              markerId: MarkerId('shipper'),
              position: LatLng(shipperLat!, shipperLng!),
              icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueBlue),
            ),
        },
      ),
    );
  }
}
```

## 9. Testing

### Test SignalR Connection
1. Connect với token hợp lệ
2. Join order group
3. Gửi location từ shipper app
4. Kiểm tra customer app có nhận được event không

### Test Location Updates
1. Shipper app: Bắt đầu tracking GPS
2. Di chuyển và kiểm tra location được gửi lên server
3. Customer app: Kiểm tra marker được update trên map

## 10. Troubleshooting

### SignalR không connect được
- Kiểm tra token có hợp lệ không
- Kiểm tra URL có đúng không (ws:// hoặc wss://)
- Kiểm tra CORS settings

### Location không được update
- Kiểm tra GPS permission
- Kiểm tra API endpoint có được gọi không
- Kiểm tra SignalR connection có active không

### Map không hiển thị shipper marker
- Kiểm tra `ShipperLocationUpdated` event có được nhận không
- Kiểm tra lat/lng có hợp lệ không
- Kiểm tra map camera position

---

**Lưu ý:** 
- SignalR endpoint: `/hubs/location`
- Token phải được gửi trong query string: `?access_token={token}`
- Server tự động broadcast location đến tất cả clients trong group `order-{orderId}`
