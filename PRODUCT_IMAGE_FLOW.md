# Product Image Flow - Tóm tắt đơn giản

## 🎯 Flow hoạt động

### 1. Upload ảnh lên Cloudinary
```
POST /api/images/upload
→ Trả về: { imageUrl: "https://res.cloudinary.com/..." }
```

### 2. Tạo Product với danh sách ImageUrls
```
POST /api/products
Body: {
  name: "Royal Canin",
  price: 1250000,
  ...
  imageUrls: [
    "https://res.cloudinary.com/.../image1.jpg",
    "https://res.cloudinary.com/.../image2.jpg",
    "https://res.cloudinary.com/.../image3.jpg"
  ]
}
```

### 3. Backend tự động tạo ProductImage records
- Ảnh đầu tiên → `isPrimary = true`, `sortOrder = 1`
- Ảnh thứ 2 → `isPrimary = false`, `sortOrder = 2`
- Ảnh thứ 3 → `isPrimary = false`, `sortOrder = 3`

## 📝 Ví dụ Frontend (JavaScript)

```javascript
// Bước 1: Upload nhiều ảnh
async function uploadImages(files) {
  const imageUrls = [];
  
  for (const file of files) {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await fetch('/api/images/upload', {
      method: 'POST',
      body: formData
    });
    
    const result = await response.json();
    if (result.code === 200) {
      imageUrls.push(result.data.imageUrl);
    }
  }
  
  return imageUrls;
}

// Bước 2: Tạo Product với imageUrls
async function createProduct(productData, imageUrls) {
  const response = await fetch('/api/products', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      ...productData,
      imageUrls: imageUrls // Gửi danh sách URLs
    })
  });
  
  return await response.json();
}

// Sử dụng
const files = [file1, file2, file3];
const imageUrls = await uploadImages(files);
const product = await createProduct({
  name: "Royal Canin Adult",
  price: 1250000,
  categoryId: 1,
  brandId: 1,
  stockQuantity: 50
}, imageUrls);
```

## ✅ Kết quả

Product được tạo với tất cả ảnh đã được lưu vào database:
- Bảng `products`: 1 record
- Bảng `product_images`: N records (N = số lượng ảnh)

Khi GET Product, sẽ trả về kèm danh sách ảnh:
```json
{
  "productId": 1,
  "name": "Royal Canin Adult",
  "images": [
    { "imageUrl": "...", "isPrimary": true, "sortOrder": 1 },
    { "imageUrl": "...", "isPrimary": false, "sortOrder": 2 },
    { "imageUrl": "...", "isPrimary": false, "sortOrder": 3 }
  ]
}
```

## 🔄 Update Product với ảnh mới

Khi update Product, nếu gửi `imageUrls` mới:
- Tất cả ảnh cũ sẽ bị xóa
- Ảnh mới sẽ được thêm vào
- Ảnh đầu tiên trong array mới sẽ là primary

```javascript
PUT /api/products/1
{
  "name": "Updated Name",
  ...
  "imageUrls": ["new-url-1", "new-url-2"] // Thay thế toàn bộ ảnh cũ
}
```

## 📌 Lưu ý

1. **ImageUrls là optional**: Có thể tạo Product không có ảnh
2. **Nhiều ảnh**: Một Product có thể có nhiều ảnh
3. **Primary image**: Ảnh đầu tiên trong array tự động là primary
4. **Sort order**: Tự động set theo thứ tự trong array
