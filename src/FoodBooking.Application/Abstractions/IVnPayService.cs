namespace FoodBooking.Application.Abstractions;

public interface IVnPayService
{
    string CreatePaymentUrl(string transactionRef, decimal amount, string orderInfo, string clientIpAddress);
    bool ValidateSignature(IDictionary<string, string> queryParams);
    bool IsSuccessResponse(IDictionary<string, string> queryParams);
}
