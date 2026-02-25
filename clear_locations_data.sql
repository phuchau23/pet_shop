-- Clear all location data
-- Xóa tất cả dữ liệu địa chỉ (wards -> districts -> provinces)

-- Xóa wards trước (vì có FK đến districts)
DELETE FROM wards;

-- Xóa districts (vì có FK đến provinces)
DELETE FROM districts;

-- Xóa provinces
DELETE FROM provinces;

-- Reset sequence nếu cần
ALTER SEQUENCE provinces_id_seq RESTART WITH 1;
ALTER SEQUENCE districts_id_seq RESTART WITH 1;
ALTER SEQUENCE wards_id_seq RESTART WITH 1;

-- Kiểm tra kết quả
SELECT COUNT(*) as province_count FROM provinces;
SELECT COUNT(*) as district_count FROM districts;
SELECT COUNT(*) as ward_count FROM wards;
