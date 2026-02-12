# Image Upload Flow - Cloudinary Integration

## 📋 Tổng quan

Hệ thống đã được tích hợp Cloudinary để upload và lưu trữ ảnh. URL ảnh sau khi upload sẽ được lưu vào database trong bảng `product_images`.

## 🔧 Cấu hình

Cloudinary settings đã được cấu hình trong `appsettings.json`:
```json
{
  "Cloudinary": {
    "CloudName": "dj1xp6sv0",
    "ApiKey": "892797351289561",
    "ApiSecret": "Feuuq62VuL-n4w8HPTz-wle3OSo"
  }
}
```

## 📡 API Endpoints

### 1. Upload Image
**Endpoint:** `POST /api/images/upload`

**Request:**
- Method: `POST`
- Content-Type: `multipart/form-data`
- Body:
  - `file` (IFormFile): File ảnh cần upload
  - Ảnh sẽ được lưu tự động vào folder `products` trên Cloudinary

**Response:**
```json
{
  "code": 200,
  "message": "Image uploaded successfully",
  "data": {
    "imageUrl": "https://res.cloudinary.com/dj1xp6sv0/image/upload/v1234567890/products/guid_filename.jpg",
    "publicId": "products/guid_filename",
    "fileName": "product-image.jpg",
    "fileSize": 123456
  }
}
```

**Validation:**
- File types: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
- Max file size: 10MB

### 2. Delete Image
**Endpoint:** `DELETE /api/images/{publicId}`

**Request:**
- Method: `DELETE`
- Path parameter: `publicId` - Public ID của ảnh trên Cloudinary

**Response:**
```json
{
  "code": 200,
  "message": "Image deleted successfully",
  "data": null
}
```

## 🔄 Flow hoạt động

### Frontend (FE) Flow:

#### Bước 1: User chọn ảnh
```javascript
// User chọn file từ input
const fileInput = document.querySelector('input[type="file"]');
const file = fileInput.files[0];

// Validate file
if (!file) {
  alert('Vui lòng chọn ảnh');
  return;
}

// Validate file type
const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
if (!allowedTypes.includes(file.type)) {
  alert('Định dạng ảnh không hợp lệ');
  return;
}

// Validate file size (10MB)
if (file.size > 10 * 1024 * 1024) {
  alert('Kích thước ảnh không được vượt quá 10MB');
  return;
}
```

#### Bước 2: Upload ảnh lên Cloudinary
```javascript
async function uploadImage(file) {
  try {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch('https://your-api-domain.com/api/images/upload', {
      method: 'POST',
      body: formData,
      // Không cần set Content-Type header, browser sẽ tự động set với boundary
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Upload failed');
    }

    const result = await response.json();
    
    if (result.code === 200) {
      // Upload thành công
      const imageUrl = result.data.imageUrl;
      const publicId = result.data.publicId;
      
      console.log('Image URL:', imageUrl);
      console.log('Public ID:', publicId);
      
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Upload error:', error);
    throw error;
  }
}

// Sử dụng
const uploadResult = await uploadImage(file, 'products');
```

#### Bước 3: Lưu URL ảnh vào Database (khi tạo/sửa Product)
```javascript
// Khi tạo Product mới với nhiều ảnh
async function createProduct(productData, imageUrls) {
  const product = {
    name: productData.name,
    description: productData.description,
    price: productData.price,
    stockQuantity: productData.stockQuantity,
    categoryId: productData.categoryId,
    brandId: productData.brandId,
    isActive: true,
    // Gửi danh sách imageUrls - backend sẽ tự động tạo ProductImage records
    imageUrls: imageUrls // Array of URLs: ["https://...", "https://...", ...]
  };

  // Tạo Product - backend sẽ tự động tạo ProductImage records từ imageUrls
  const productResponse = await fetch('/api/products', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(product)
  });

  const productResult = await productResponse.json();
  // Product đã được tạo kèm tất cả ảnh trong productResult.data.images
  return productResult.data;
}
```

### Backend (BE) Flow:

#### Bước 1: Nhận request upload từ FE
```
POST /api/images/upload
Content-Type: multipart/form-data
Body: { file: File }
```

#### Bước 2: Validate file
- Kiểm tra file có tồn tại không
- Kiểm tra file type (jpg, jpeg, png, gif, webp)
- Kiểm tra file size (max 10MB)

#### Bước 3: Upload lên Cloudinary
```csharp
// ImageEndpoints.cs
var uploadResult = await imageService.UploadImageAsync(
    stream, 
    file.FileName, 
    cancellationToken);
```

#### Bước 4: CloudinaryImageService xử lý
```csharp
// CloudinaryImageService.cs
public async Task<UploadedImageResult> UploadImageAsync(...)
{
    const string folder = "products"; // Hardcode folder
    
    // 1. Tạo unique public ID
    var publicId = $"{folder}/{Guid.NewGuid()}_{fileName}";
    
    // 2. Upload lên Cloudinary
    var uploadParams = new ImageUploadParams
    {
        File = new FileDescription(fileName, imageStream),
        PublicId = publicId,
        Folder = folder,
        Overwrite = false
    };
    
    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
    
    // 3. Trả về URL và PublicId
    return new UploadedImageResult
    {
        ImageUrl = uploadResult.SecureUrl.ToString(),
        PublicId = uploadResult.PublicId
    };
}
```

#### Bước 5: Trả về response cho FE
```json
{
  "code": 200,
  "message": "Image uploaded successfully",
  "data": {
    "imageUrl": "https://res.cloudinary.com/...",
    "publicId": "products/guid_filename",
    "fileName": "product.jpg",
    "fileSize": 123456
  }
}
```

#### Bước 6: FE tạo Product với ImageUrls
FE gửi request tạo Product kèm danh sách `imageUrls`. Backend tự động tạo ProductImage records.

**Request:**
```json
POST /api/products
{
  "name": "Royal Canin Adult",
  "description": "Thức ăn cho chó",
  "price": 1250000,
  "stockQuantity": 50,
  "categoryId": 1,
  "brandId": 1,
  "isActive": true,
  "imageUrls": [
    "https://res.cloudinary.com/dj1xp6sv0/image/upload/.../image1.jpg",
    "https://res.cloudinary.com/dj1xp6sv0/image/upload/.../image2.jpg",
    "https://res.cloudinary.com/dj1xp6sv0/image/upload/.../image3.jpg"
  ]
}
```

**Backend xử lý:**
- Tạo Product record
- Tự động tạo ProductImage records từ `imageUrls`
- Ảnh đầu tiên được đánh dấu `isPrimary = true`
- `sortOrder` được set theo thứ tự trong array (1, 2, 3, ...)

**Response:**
```json
{
  "code": 201,
  "message": "Product created successfully",
  "data": {
    "productId": 1,
    "name": "Royal Canin Adult",
    "images": [
      {
        "productImageId": 1,
        "imageUrl": "https://res.cloudinary.com/.../image1.jpg",
        "isPrimary": true,
        "sortOrder": 1
      },
      {
        "productImageId": 2,
        "imageUrl": "https://res.cloudinary.com/.../image2.jpg",
        "isPrimary": false,
        "sortOrder": 2
      }
    ]
  }
}
```

## 📝 Flow hoàn chỉnh - Ví dụ Frontend

### Bước 1: Upload nhiều ảnh
```javascript
async function uploadMultipleImages(files) {
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
  
  return imageUrls; // ["https://...", "https://...", ...]
}
```

### Bước 2: Tạo Product với ImageUrls
```javascript
async function createProductWithImages(productData, imageUrls) {
  const product = {
    ...productData,
    imageUrls: imageUrls // Gửi danh sách URLs
  };

  const response = await fetch('/api/products', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(product)
  });

  return await response.json();
}

// Sử dụng
const files = [file1, file2, file3];
const imageUrls = await uploadMultipleImages(files);
const product = await createProductWithImages({
  name: "Royal Canin",
  price: 1250000,
  // ... other fields
}, imageUrls);
```

## 📝 Ví dụ hoàn chỉnh (React/TypeScript)

```typescript
// ImageUpload.tsx
import React, { useState } from 'react';

interface UploadResult {
  imageUrl: string;
  publicId: string;
  fileName: string;
  fileSize: number;
}

const ImageUpload: React.FC = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState<UploadResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        setError('Định dạng ảnh không hợp lệ');
        return;
      }
      
      if (file.size > 10 * 1024 * 1024) {
        setError('Kích thước ảnh không được vượt quá 10MB');
        return;
      }
      
      setSelectedFile(file);
      setError(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    setUploading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append('file', selectedFile);

      const response = await fetch('https://your-api-domain.com/api/images/upload', {
        method: 'POST',
        body: formData,
      });

      const result = await response.json();

      if (result.code === 200) {
        setUploadResult(result.data);
        // Lưu imageUrl vào state để dùng khi tạo Product
        console.log('Upload thành công:', result.data.imageUrl);
        // Có thể thêm vào danh sách imageUrls để tạo Product sau
      } else {
        setError(result.message || 'Upload thất bại');
      }
    } catch (err) {
      setError('Có lỗi xảy ra khi upload ảnh');
      console.error(err);
    } finally {
      setUploading(false);
    }
  };

  // Hàm tạo Product với nhiều ảnh
  const handleCreateProduct = async (productData: any, imageUrls: string[]) => {
    try {
      const response = await fetch('https://your-api-domain.com/api/products', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          ...productData,
          imageUrls: imageUrls // Gửi danh sách URLs
        })
      });

      const result = await response.json();
      if (result.code === 201) {
        console.log('Product created with images:', result.data);
      }
    } catch (err) {
      console.error('Error creating product:', err);
    }
  };

  return (
    <div>
      <input type="file" accept="image/*" onChange={handleFileChange} />
      <button onClick={handleUpload} disabled={!selectedFile || uploading}>
        {uploading ? 'Đang upload...' : 'Upload ảnh'}
      </button>
      
      {error && <p style={{ color: 'red' }}>{error}</p>}
      
      {uploadResult && (
        <div>
          <p>Upload thành công!</p>
          <img src={uploadResult.imageUrl} alt="Uploaded" style={{ maxWidth: '300px' }} />
          <p>URL: {uploadResult.imageUrl}</p>
        </div>
      )}
    </div>
  );
};

export default ImageUpload;
```

## 🔐 Authentication

**Lưu ý:** Hiện tại endpoint upload không yêu cầu authentication. Nếu muốn bảo mật, có thể thêm `[Authorize]` attribute vào endpoint.

## 📌 Lưu ý quan trọng

1. **File Size Limit:** Tối đa 10MB
2. **File Types:** Chỉ chấp nhận: jpg, jpeg, png, gif, webp
3. **Folder Structure:** Ảnh được lưu tự động trong folder `products` trên Cloudinary
4. **Public ID:** Mỗi ảnh có một Public ID duy nhất, dùng để xóa ảnh sau này
5. **URL Format:** Cloudinary trả về HTTPS URL an toàn

## 🗄️ Database Schema

Sau khi tạo Product với `imageUrls`, backend tự động tạo records trong bảng `product_images`:

```sql
-- Khi tạo Product với imageUrls = ["url1", "url2", "url3"]
-- Backend tự động tạo:

INSERT INTO products (name, price, ...) VALUES ('Royal Canin', 1250000, ...);
-- ProductId = 1

INSERT INTO product_images (product_id, image_url, is_primary, sort_order)
VALUES 
  (1, 'url1', true, 1),   -- Ảnh đầu tiên là primary
  (1, 'url2', false, 2),
  (1, 'url3', false, 3);
```

**Lưu ý:**
- Ảnh đầu tiên trong array (`imageUrls[0]`) sẽ có `isPrimary = true`
- `sortOrder` được set theo index trong array (1, 2, 3, ...)
- Nếu `imageUrls` là `null` hoặc empty, Product sẽ không có ảnh

## 🧪 Test với Postman/Thunder Client

1. **Method:** POST
2. **URL:** `http://localhost:5000/api/images/upload`
3. **Body:** 
   - Type: `form-data`
   - Key: `file` (type: File)
4. **Send**

## ✅ Checklist

- [x] Cloudinary package đã được thêm
- [x] CloudinarySettings đã được cấu hình
- [x] IImageService interface đã được tạo
- [x] CloudinaryImageService implementation đã hoàn thành
- [x] Upload endpoint đã được tạo
- [x] Delete endpoint đã được tạo
- [x] Validation đã được thêm
- [x] Error handling đã được xử lý
