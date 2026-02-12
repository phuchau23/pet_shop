namespace FoodBooking.Application.Common;

public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value > 0 ? value : 1;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 0 ? (value > MaxPageSize ? MaxPageSize : value) : 10;
    }

    public string? SearchTerm { get; set; }
}
