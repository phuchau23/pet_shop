using System.Net;
using System.Security.Cryptography;
using System.Text;
using FoodBooking.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace FoodBooking.Infrastructure.External.Payments;

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _configuration;

    public VnPayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreatePaymentUrl(string transactionRef, decimal amount, string orderInfo, string clientIpAddress)
    {
        var tmnCode = _configuration["VNPay:TmnCode"] ?? throw new InvalidOperationException("VNPay TmnCode not configured");
        var hashSecret = _configuration["VNPay:HashSecret"] ?? throw new InvalidOperationException("VNPay HashSecret not configured");
        var payUrl = _configuration["VNPay:PayUrl"] ?? throw new InvalidOperationException("VNPay PayUrl not configured");
        var returnUrl = _configuration["VNPay:ReturnUrl"] ?? throw new InvalidOperationException("VNPay ReturnUrl not configured");

        var createDate = DateTime.UtcNow;
        var expireDate = createDate.AddMinutes(15);
        var amountVnd = decimal.ToInt64(amount * 100);

        var vnPayData = new SortedDictionary<string, string>
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = tmnCode,
            ["vnp_Amount"] = amountVnd.ToString(),
            ["vnp_CreateDate"] = createDate.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = string.IsNullOrWhiteSpace(clientIpAddress) ? "127.0.0.1" : clientIpAddress,
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = orderInfo,
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = transactionRef,
            ["vnp_ExpireDate"] = expireDate.ToString("yyyyMMddHHmmss")
        };

        var query = BuildQuery(vnPayData);
        var secureHash = ComputeHmacSha512(hashSecret, query);

        return $"{payUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public bool ValidateSignature(IDictionary<string, string> queryParams)
    {
        var hashSecret = _configuration["VNPay:HashSecret"] ?? throw new InvalidOperationException("VNPay HashSecret not configured");

        if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash) || string.IsNullOrWhiteSpace(receivedHash))
        {
            return false;
        }

        var data = new SortedDictionary<string, string>(
            queryParams
                .Where(p => p.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                .Where(p => p.Key != "vnp_SecureHash" && p.Key != "vnp_SecureHashType")
                .ToDictionary(p => p.Key, p => p.Value)
        );

        var query = BuildQuery(data);
        var computedHash = ComputeHmacSha512(hashSecret, query);
        return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsSuccessResponse(IDictionary<string, string> queryParams)
    {
        queryParams.TryGetValue("vnp_ResponseCode", out var responseCode);
        queryParams.TryGetValue("vnp_TransactionStatus", out var transactionStatus);

        return responseCode == "00" && (string.IsNullOrEmpty(transactionStatus) || transactionStatus == "00");
    }

    private static string BuildQuery(SortedDictionary<string, string> data)
    {
        return string.Join("&", data.Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
    }

    private static string ComputeHmacSha512(string key, string input)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(input);

        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(inputBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
