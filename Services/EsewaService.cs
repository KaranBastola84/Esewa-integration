using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EsewaBackend.Models;

namespace EsewaBackend.Services;

public class EsewaService : IEsewaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EsewaConfig _config;
    private readonly ILogger<EsewaService> _logger;

    public EsewaService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<EsewaService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _config = configuration.GetSection("Esewa").Get<EsewaConfig>()
                  ?? throw new Exception("Esewa configuration missing");
    }

    // ================= INITIATE PAYMENT =================
    public Task<PaymentResponse> InitiatePayment(PaymentRequest request)
    {
        try
        {
            var transactionUuid = Guid.NewGuid().ToString("N");

            var totalAmount =
                request.Amount +
                request.TaxAmount +
                request.ServiceCharge +
                request.DeliveryCharge;

            var message =
                $"total_amount={totalAmount:0.00}," +
                $"transaction_uuid={transactionUuid}," +
                $"product_code={_config.ProductCode}";

            var signature = GenerateSignature(message);

            var formData = new Dictionary<string, string>
            {
                ["amount"] = request.Amount.ToString("0.00"),
                ["tax_amount"] = request.TaxAmount.ToString("0.00"),
                ["total_amount"] = totalAmount.ToString("0.00"),
                ["transaction_uuid"] = transactionUuid,
                ["product_code"] = _config.ProductCode,
                ["product_service_charge"] = request.ServiceCharge.ToString("0.00"),
                ["product_delivery_charge"] = request.DeliveryCharge.ToString("0.00"),
                ["success_url"] = _config.SuccessUrl,
                ["failure_url"] = _config.FailureUrl,
                ["signed_field_names"] = _config.SignedFieldNames,
                ["signature"] = signature
            };

            return Task.FromResult(new PaymentResponse
            {
                Success = true,
                Message = "Payment initiated",
                TransactionId = transactionUuid,
                PaymentUrl = _config.PaymentUrl,
                FormData = formData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "eSewa payment initiation failed");
            return Task.FromResult(new PaymentResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ================= VERIFY PAYMENT =================
    public async Task<VerificationResponse> VerifyPayment(VerificationRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            var url =
                $"{_config.VerificationUrl}?" +
                $"product_code={_config.ProductCode}&" +
                $"total_amount={request.TotalAmount:0.00}&" +
                $"transaction_uuid={request.TransactionUuid}";

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("eSewa verification response: {Json}", json);

            if (!response.IsSuccessStatusCode)
            {
                return FailVerification("Verification API failed", json);
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var status = root.GetProperty("status").GetString() ?? "UNKNOWN";

            return new VerificationResponse
            {
                Success = status == "COMPLETE",
                Status = status,
                TransactionId = root.GetProperty("transaction_uuid").GetString() ?? "",
                RefId = root.TryGetProperty("ref_id", out var refId)
                            ? refId.GetString() ?? ""
                            : "",
                Amount = request.TotalAmount,
                Message = status == "COMPLETE"
                            ? "Payment verified successfully"
                            : "Payment not completed",
                RawResponse = json
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "eSewa verification error");
            return new VerificationResponse
            {
                Success = false,
                Status = "ERROR",
                Message = ex.Message
            };
        }
    }

    // ================= SIGNATURE =================
    private string GenerateSignature(string message)
    {
        using var hmac =
            new HMACSHA256(Encoding.UTF8.GetBytes(_config.SecretKey));

        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    private static VerificationResponse FailVerification(string msg, string raw) =>
        new()
        {
            Success = false,
            Status = "FAILED",
            Message = msg,
            RawResponse = raw
        };
}
