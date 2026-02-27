namespace FoodBooking.Domain.Entities;

public class Voucher
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // Mã voucher (unique)
    public string Name { get; set; } = string.Empty; // Tên voucher
    public string? Description { get; set; } // Mô tả
    
    // Loại giảm giá
    public string DiscountType { get; set; } = "percentage"; // "percentage" | "fixed_amount"
    public decimal DiscountValue { get; set; } // Giá trị giảm (%, hoặc số tiền)
    
    // Điều kiện áp dụng
    public decimal? MinOrderAmount { get; set; } // Đơn hàng tối thiểu
    public decimal? MaxDiscountAmount { get; set; } // Giảm tối đa (nếu là %)
    
    // Số lượng và thời gian
    public int? UsageLimit { get; set; } // Giới hạn số lần sử dụng (null = không giới hạn)
    public int UsedCount { get; set; } = 0; // Số lần đã sử dụng
    public DateTime? StartDate { get; set; } // Ngày bắt đầu
    public DateTime? EndDate { get; set; } // Ngày kết thúc
    
    // Trạng thái
    public bool IsActive { get; set; } = true;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
