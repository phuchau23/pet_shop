namespace FoodBooking.Application.Common;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> Success(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Error(int code, string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Message = message,
            Data = default,
            Errors = errors
        };
    }
}