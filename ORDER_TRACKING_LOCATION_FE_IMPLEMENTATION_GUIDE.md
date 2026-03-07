# Hướng Dẫn FE: Order Tracking với Location Realtime

## 📋 Tổng Quan

Hệ thống tracking đơn hàng với location realtime cho phép:
- **Customer** xem vị trí shipper trên map realtime
- **Shipper** cập nhật vị trí mỗi 5 giây khi đang giao hàng
- **Tracking API** trả về đầy đủ location data (shop, customer, shipper) để hiển thị map

## 🔄 Flow Hoàn Chỉnh

```
1. Customer đặt hàng
   → Order được tạo với customerLat, customerLng (đã lưu sẵn)
   → Order status = "pending"
   ↓
2. Shipper xem danh sách available orders
   → GET /api/orders/shipper/available
   ↓
3. Shipper nhận đơn (với vị trí hiện tại)
   → PATCH /api/orders/{id}/shipper-status
   Body: { shipperId, status: "shipping", lat, lng }
   → Lưu vị trí ban đầu của shipper vào DB
   → SignalR notify customer: "ShipperAssigned" + "ShipperLocationUpdated"
   ↓
4. Shipper app bắt đầu tracking GPS
   → Gọi POST /api/orders/{id}/shipper-location mỗi 5 giây
   → Server broadcast qua SignalR: "ShipperLocationUpdated"
   ↓
5. Customer app nhận location updates qua SignalR
   → Update marker shipper trên map realtime
   → Hoặc poll GET /api/orders/{id}/tracking để lấy location mới nhất
   ↓
6. Shipper đến nơi → Confirm delivered
   → PATCH /api/orders/{id}/shipper-status với status: "delivered"
```

---

## 🚀 API Endpoints

### 1. Shipper Nhận Đơn (với Location Ban Đầu)

**Endpoint:**
```
PATCH /api/orders/{id}/shipper-status
```

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body:**
```json
{
  "shipperId": 3,
  "status": "shipping",
  "lat": 21.0285,    // ⭐ Vị trí shipper khi nhận đơn (bắt buộc)
  "lng": 105.8542    // ⭐ Vị trí shipper khi nhận đơn (bắt buộc)
}
```

**Response (200 OK):**
```json
{
  "code": 200,
  "message": "Order status updated successfully by shipper",
  "data": {
    "id": 6,
    "status": "shipping",
    "shipperId": 3,
    "shipperCurrentLat": 21.0285,
    "shipperCurrentLng": 105.8542,
    "shipperLocationUpdatedAt": "2024-01-01T10:00:00Z",
    ...
  }
}
```

**Lưu ý:**
- `lat` và `lng` là **bắt buộc** khi `status = "shipping"` (shipper nhận đơn)
- Server sẽ tự động lưu vị trí này vào `shipperCurrentLat`, `shipperCurrentLng`
- SignalR sẽ tự động notify customer về shipper assignment và location ban đầu

---

### 2. Shipper Cập Nhật Location (Realtime Tracking)

**Endpoint:**
```
POST /api/orders/{id}/shipper-location
```

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body:**
```json
{
  "lat": 21.0290,
  "lng": 105.8545
}
```

**Response (200 OK):**
```json
{
  "code": 200,
  "message": "Shipper location updated successfully",
  "data": null
}
```

**Lưu ý:**
- Gọi API này **mỗi 5 giây** khi shipper đang di chuyển
- Server sẽ:
  1. Cập nhật location trong database
  2. Broadcast qua SignalR đến customer đang track order này
- Customer sẽ nhận SignalR event `ShipperLocationUpdated` realtime

---

### 3. Customer Tracking Order (Lấy Location Data)

**Endpoint:**
```
GET /api/orders/{id}/tracking
```

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
```

**Response (200 OK):**
```json
{
  "code": 200,
  "message": "Order tracking retrieved successfully",
  "data": {
    "orderId": 6,
    "currentStatus": "shipping",
    "statusDisplayName": "Đang giao hàng",
    "statusDescription": "Đơn hàng đang được giao đến bạn",
    "shipperId": 3,
    
    // ⭐ LOCATION DATA CHO MAP
    "shopLat": 10.841449,           // Vị trí shop (luôn có giá trị)
    "shopLng": 106.809997,          // Vị trí shop (luôn có giá trị)
    "customerLat": 21.0300,         // Vị trí customer (đã lưu lúc tạo order)
    "customerLng": 105.8600,        // Vị trí customer (đã lưu lúc tạo order)
    "shipperCurrentLat": 21.0285,   // Vị trí shipper hiện tại (realtime)
    "shipperCurrentLng": 105.8542, // Vị trí shipper hiện tại (realtime)
    
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T11:00:00Z",
    "timeline": [
      {
        "status": "pending",
        "statusDisplayName": "Chờ xác nhận",
        "description": "Đơn hàng đang chờ được xác nhận",
        "timestamp": "2024-01-01T10:00:00Z",
        "isCompleted": true,
        "isCurrent": false
      },
      {
        "status": "shipping",
        "statusDisplayName": "Đang giao hàng",
        "description": "Đơn hàng đang được giao đến bạn",
        "timestamp": "2024-01-01T11:00:00Z",
        "isCompleted": false,
        "isCurrent": true
      }
    ]
  }
}
```

**Lưu ý:**
- `shopLat`, `shopLng`: Luôn có giá trị (default: FPT University HCM)
- `customerLat`, `customerLng`: Có giá trị nếu đã geocode lúc tạo order
- `shipperCurrentLat`, `shipperCurrentLng`: Có giá trị khi shipper đã nhận đơn và update location
- Có thể null nếu shipper chưa update location

---

## 📡 SignalR Integration

### Connection

**Endpoint:**
```
ws://localhost:5000/hubs/location?access_token={JWT_TOKEN}
```

**Authentication:**
- Token được truyền qua query string `access_token`
- Không cần prefix "Bearer"
- Token phải là JWT token hợp lệ từ login API

### Flutter/Dart Implementation

```dart
import 'package:signalr_netcore/signalr_client.dart';

class SignalRService {
  late HubConnection _connection;
  String? _token;
  int? _currentOrderId;

  // Connect to SignalR hub
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

    // Register event handlers
    _connection.on('ShipperAssigned', _handleShipperAssigned);
    _connection.on('ShipperLocationUpdated', _handleLocationUpdated);
    _connection.on('OrderStatusChanged', _handleStatusChanged);

    await _connection.start();
    print('SignalR Connected');
  }

  // Join order tracking group
  Future<void> joinOrderTracking(int orderId) async {
    _currentOrderId = orderId;
    await _connection.invoke('JoinOrderTracking', args: [orderId]);
    print('Joined tracking group for order $orderId');
  }

  // Leave tracking group
  Future<void> leaveOrderTracking() async {
    if (_currentOrderId != null) {
      await _connection.invoke('LeaveOrderTracking', args: [_currentOrderId]);
      _currentOrderId = null;
    }
  }

  // Handle shipper assigned event
  void _handleShipperAssigned(List<Object?>? args) {
    if (args == null || args.isEmpty) return;
    
    final data = args[0] as Map<String, dynamic>;
    final orderId = data['orderId'] as int;
    final shipperId = data['shipperId'] as int;
    final lat = data['lat'] as double?;
    final lng = data['lng'] as double?;
    
    print('Shipper $shipperId assigned to order $orderId');
    print('Initial location: $lat, $lng');
    
    // Update UI: Show shipper marker on map
    // Notify user: "Shipper đã nhận đơn của bạn"
  }

  // Handle location updated event
  void _handleLocationUpdated(List<Object?>? args) {
    if (args == null || args.isEmpty) return;
    
    final data = args[0] as Map<String, dynamic>;
    final orderId = data['orderId'] as int;
    final lat = data['lat'] as double;
    final lng = data['lng'] as double;
    final timestamp = data['timestamp'] as String;
    
    print('Shipper location updated for order $orderId: $lat, $lng');
    
    // Update shipper marker position on map
    // Animate marker movement if needed
  }

  // Handle status changed event
  void _handleStatusChanged(List<Object?>? args) {
    if (args == null || args.isEmpty) return;
    
    final data = args[0] as Map<String, dynamic>;
    final orderId = data['orderId'] as int;
    final status = data['status'] as String;
    final statusDisplayName = data['statusDisplayName'] as String;
    
    print('Order $orderId status changed to: $statusDisplayName');
    
    // Update order status in UI
  }

  // Disconnect
  Future<void> disconnect() async {
    await leaveOrderTracking();
    await _connection.stop();
    print('SignalR Disconnected');
  }
}
```

---

## 🗺️ Map Implementation

### Customer App - Tracking Map

```dart
import 'package:google_maps_flutter/google_maps_flutter.dart';

class OrderTrackingMap extends StatefulWidget {
  final int orderId;
  final String token;

  @override
  State<OrderTrackingMap> createState() => _OrderTrackingMapState();
}

class _OrderTrackingMapState extends State<OrderTrackingMap> {
  GoogleMapController? _mapController;
  SignalRService? _signalRService;
  
  // Map markers
  Marker? _shopMarker;
  Marker? _customerMarker;
  Marker? _shipperMarker;
  
  // Location data
  double? shopLat;
  double? shopLng;
  double? customerLat;
  double? customerLng;
  double? shipperLat;
  double? shipperLng;

  @override
  void initState() {
    super.initState();
    _initializeTracking();
  }

  Future<void> _initializeTracking() async {
    // 1. Connect SignalR
    _signalRService = SignalRService();
    await _signalRService!.connect(widget.token);
    await _signalRService!.joinOrderTracking(widget.orderId);
    
    // 2. Fetch initial tracking data
    await _fetchTrackingData();
    
    // 3. Setup SignalR listeners
    _setupSignalRListeners();
  }

  Future<void> _fetchTrackingData() async {
    // Call GET /api/orders/{id}/tracking
    final response = await http.get(
      Uri.parse('http://localhost:5000/api/orders/${widget.orderId}/tracking'),
      headers: {
        'Authorization': 'Bearer ${widget.token}',
      },
    );
    
    final data = json.decode(response.body)['data'];
    
    setState(() {
      shopLat = data['shopLat'] as double;
      shopLng = data['shopLng'] as double;
      customerLat = data['customerLat'] as double?;
      customerLng = data['customerLng'] as double?;
      shipperLat = data['shipperCurrentLat'] as double?;
      shipperLng = data['shipperCurrentLng'] as double?;
    });
    
    _updateMarkers();
  }

  void _setupSignalRListeners() {
    // Listen for location updates
    _signalRService!.onLocationUpdated = (lat, lng) {
      setState(() {
        shipperLat = lat;
        shipperLng = lng;
      });
      _updateShipperMarker(lat, lng);
    };
  }

  void _updateMarkers() {
    final markers = <Marker>{};
    
    // Shop marker (green)
    if (shopLat != null && shopLng != null) {
      _shopMarker = Marker(
        markerId: MarkerId('shop'),
        position: LatLng(shopLat!, shopLng!),
        icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueGreen),
        infoWindow: InfoWindow(title: 'Cửa hàng'),
      );
      markers.add(_shopMarker!);
    }
    
    // Customer marker (blue)
    if (customerLat != null && customerLng != null) {
      _customerMarker = Marker(
        markerId: MarkerId('customer'),
        position: LatLng(customerLat!, customerLng!),
        icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueBlue),
        infoWindow: InfoWindow(title: 'Địa chỉ giao hàng'),
      );
      markers.add(_customerMarker!);
    }
    
    // Shipper marker (red) - only if location available
    if (shipperLat != null && shipperLng != null) {
      _shipperMarker = Marker(
        markerId: MarkerId('shipper'),
        position: LatLng(shipperLat!, shipperLng!),
        icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueRed),
        infoWindow: InfoWindow(title: 'Shipper đang giao hàng'),
      );
      markers.add(_shipperMarker!);
    }
    
    setState(() {
      // Update map with new markers
    });
  }

  void _updateShipperMarker(double lat, double lng) {
    // Animate shipper marker movement
    _shipperMarker = Marker(
      markerId: MarkerId('shipper'),
      position: LatLng(lat, lng),
      icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueRed),
      infoWindow: InfoWindow(title: 'Shipper đang giao hàng'),
    );
    
    // Animate camera to follow shipper
    _mapController?.animateCamera(
      CameraUpdate.newLatLng(LatLng(lat, lng)),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (shopLat == null || shopLng == null) {
      return Center(child: CircularProgressIndicator());
    }
    
    return GoogleMap(
      initialCameraPosition: CameraPosition(
        target: LatLng(shopLat!, shopLng!),
        zoom: 13,
      ),
      markers: {
        if (_shopMarker != null) _shopMarker!,
        if (_customerMarker != null) _customerMarker!,
        if (_shipperMarker != null) _shipperMarker!,
      },
      onMapCreated: (controller) {
        _mapController = controller;
      },
    );
  }

  @override
  void dispose() {
    _signalRService?.disconnect();
    super.dispose();
  }
}
```

---

## 🚴 Shipper App - Location Tracking

### Shipper nhận đơn và bắt đầu tracking

```dart
import 'package:geolocator/geolocator.dart';
import 'package:http/http.dart' as http;
import 'dart:async';
import 'dart:convert';

class ShipperTrackingService {
  String? _token;
  int? _currentOrderId;
  Timer? _locationUpdateTimer;
  StreamSubscription<Position>? _positionStream;

  // Shipper nhận đơn với vị trí hiện tại
  Future<void> acceptOrder(int orderId, String token) async {
    _token = token;
    _currentOrderId = orderId;
    
    // Lấy vị trí hiện tại
    final position = await Geolocator.getCurrentPosition();
    
    // Gọi API nhận đơn với location
    final response = await http.patch(
      Uri.parse('http://localhost:5000/api/orders/$orderId/shipper-status'),
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
      body: json.encode({
        'shipperId': getCurrentUserId(), // Lấy từ token hoặc state
        'status': 'shipping',
        'lat': position.latitude,
        'lng': position.longitude,
      }),
    );
    
    if (response.statusCode == 200) {
      print('Order accepted successfully');
      // Bắt đầu tracking location
      _startLocationTracking(orderId);
    }
  }

  // Bắt đầu tracking location mỗi 5 giây
  void _startLocationTracking(int orderId) {
    // Option 1: Sử dụng timer (polling)
    _locationUpdateTimer = Timer.periodic(Duration(seconds: 5), (timer) async {
      await _updateLocation(orderId);
    });
    
    // Option 2: Sử dụng position stream (recommended)
    _positionStream = Geolocator.getPositionStream(
      locationSettings: LocationSettings(
        accuracy: LocationAccuracy.high,
        distanceFilter: 10, // Update khi di chuyển 10m
      ),
    ).listen((position) {
      _updateLocation(orderId, position.latitude, position.longitude);
    });
  }

  // Cập nhật location lên server
  Future<void> _updateLocation(int orderId, [double? lat, double? lng]) async {
    // Nếu không có lat/lng, lấy vị trí hiện tại
    if (lat == null || lng == null) {
      final position = await Geolocator.getCurrentPosition();
      lat = position.latitude;
      lng = position.longitude;
    }
    
    try {
      final response = await http.post(
        Uri.parse('http://localhost:5000/api/orders/$orderId/shipper-location'),
        headers: {
          'Authorization': 'Bearer $_token',
          'Content-Type': 'application/json',
        },
        body: json.encode({
          'lat': lat,
          'lng': lng,
        }),
      );
      
      if (response.statusCode == 200) {
        print('Location updated: $lat, $lng');
      }
    } catch (e) {
      print('Error updating location: $e');
      // Retry logic hoặc queue location updates
    }
  }

  // Dừng tracking
  void stopTracking() {
    _locationUpdateTimer?.cancel();
    _positionStream?.cancel();
    _currentOrderId = null;
  }

  // Confirm delivered
  Future<void> confirmDelivered(int orderId) async {
    stopTracking();
    
    final response = await http.patch(
      Uri.parse('http://localhost:5000/api/orders/$orderId/shipper-status'),
      headers: {
        'Authorization': 'Bearer $_token',
        'Content-Type': 'application/json',
      },
      body: json.encode({
        'shipperId': getCurrentUserId(),
        'status': 'delivered',
      }),
    );
    
    if (response.statusCode == 200) {
      print('Order delivered successfully');
    }
  }
}
```

---

## 📱 UI/UX Recommendations

### Customer App

1. **Tracking Screen:**
   - Hiển thị map với 3 markers: Shop (green), Customer (blue), Shipper (red)
   - Draw polyline từ shop → shipper → customer
   - Hiển thị timeline order status
   - Button "Refresh" để poll API nếu không dùng SignalR

2. **Real-time Updates:**
   - Sử dụng SignalR để nhận location updates realtime
   - Animate shipper marker khi di chuyển
   - Show notification khi shipper nhận đơn
   - Show ETA (estimated time of arrival) dựa trên khoảng cách

3. **Fallback:**
   - Nếu SignalR disconnect, fallback về polling API mỗi 5-10 giây
   - Show loading indicator khi đang fetch data

### Shipper App

1. **Accept Order Screen:**
   - Hiển thị map với route từ vị trí hiện tại → customer address
   - Button "Nhận đơn" sẽ tự động lấy GPS và gọi API
   - Show confirmation dialog trước khi accept

2. **Delivery Screen:**
   - Hiển thị map với route navigation
   - Auto-start location tracking khi accept order
   - Show "Đang giao hàng" với location indicator
   - Button "Đã giao hàng" để confirm delivered

3. **Location Tracking:**
   - Background location tracking (kể cả khi app minimize)
   - Queue location updates nếu mất mạng
   - Show notification nếu tracking bị dừng

---

## ⚠️ Error Handling

### API Errors

```dart
try {
  final response = await http.patch(...);
  
  if (response.statusCode == 200) {
    // Success
  } else if (response.statusCode == 401) {
    // Unauthorized - Token expired, redirect to login
  } else if (response.statusCode == 403) {
    // Forbidden - Shipper không có quyền
  } else if (response.statusCode == 404) {
    // Order not found
  } else {
    // Other errors
  }
} catch (e) {
  // Network error, timeout, etc.
  // Retry logic hoặc show error message
}
```

### SignalR Errors

```dart
_connection.onclose((error) {
  print('SignalR connection closed: $error');
  // Auto-reconnect sẽ tự động retry
  // Hoặc manually reconnect
});

_connection.onreconnecting((error) {
  print('SignalR reconnecting...');
  // Show "Đang kết nối lại..." message
});

_connection.onreconnected((connectionId) {
  print('SignalR reconnected');
  // Rejoin tracking group
  if (_currentOrderId != null) {
    joinOrderTracking(_currentOrderId!);
  }
});
```

### Location Errors

```dart
// Check location permissions
bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
if (!serviceEnabled) {
  // Show dialog: "Vui lòng bật GPS"
  return;
}

LocationPermission permission = await Geolocator.checkPermission();
if (permission == LocationPermission.denied) {
  permission = await Geolocator.requestPermission();
  if (permission == LocationPermission.denied) {
    // Show dialog: "Cần quyền truy cập vị trí"
    return;
  }
}
```

---

## 🧪 Testing Checklist

### Customer App
- [ ] Connect SignalR thành công
- [ ] Nhận notification khi shipper nhận đơn
- [ ] Map hiển thị đúng 3 markers (shop, customer, shipper)
- [ ] Shipper marker di chuyển realtime khi shipper update location
- [ ] Polling API fallback hoạt động khi SignalR disconnect
- [ ] Timeline order status hiển thị đúng

### Shipper App
- [ ] Lấy GPS location khi nhận đơn
- [ ] API accept order thành công với location
- [ ] Location tracking tự động start khi accept order
- [ ] Location update mỗi 5 giây hoạt động đúng
- [ ] Background location tracking hoạt động
- [ ] Confirm delivered thành công

---

## 📝 Notes

1. **Location Accuracy:**
   - Sử dụng `LocationAccuracy.high` cho shipper tracking
   - Có thể giảm frequency nếu battery thấp

2. **Battery Optimization:**
   - Sử dụng `distanceFilter` để chỉ update khi di chuyển đủ xa
   - Stop tracking khi order delivered hoặc cancelled

3. **Network Optimization:**
   - Queue location updates nếu mất mạng
   - Batch updates khi reconnect
   - Retry failed requests

4. **Privacy:**
   - Chỉ track location khi order status = "shipping"
   - Stop tracking ngay khi delivered hoặc cancelled
   - Clear location data khi không cần thiết

---

## 🔗 Related Documentation

- [SignalR Realtime Tracking Guide](./SIGNALR_REALTIME_TRACKING_FE_GUIDE.md)
- [Order Tracking Guide](./ORDER_TRACKING_FE_GUIDE.md)
- [API Documentation](./API_DOCUMENTATION.md)

---

## ❓ FAQ

**Q: Tại sao `shipperCurrentLat`, `shipperCurrentLng` có thể null?**
A: Chỉ có giá trị khi shipper đã nhận đơn và update location. Nếu null, có nghĩa là shipper chưa nhận đơn hoặc chưa update location.

**Q: Có cần poll API nếu đã dùng SignalR?**
A: Không bắt buộc, nhưng nên có fallback polling mỗi 10-15 giây để đảm bảo data sync nếu SignalR disconnect.

**Q: Shipper có cần gọi API location update khi đứng yên?**
A: Có thể skip nếu location không đổi, nhưng để đơn giản có thể gọi mỗi 5 giây bất kể location có đổi hay không.

**Q: Làm sao biết shipper đã đến nơi?**
A: Khi shipper confirm delivered, status sẽ đổi sang "delivered". Customer có thể check status hoặc nhận SignalR notification.

---

**Chúc FE team implement thành công! 🚀**
