using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using EsewaBackend.Models;

namespace EsewaBackend.Services;

public class EsewaService : IEsewaService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EsewaConfig _esewaConfig;
    private readonly ILogger<EsewaService> _logger;

    public EsewaService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<EsewaService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _esewaConfig = configuration.GetSection("Esewa").Get<EsewaConfig>() ?? new EsewaConfig();
    }

    public async Task<PaymentResponse> InitiatePayment(PaymentRequest request)
    {
        try
        {
            var transactionId = Guid.NewGuid().ToString("N");
            var totalAmount = request.Amount + request.TaxAmount + request.ServiceCharge + request.DeliveryCharge;

            // For eSewa v2 API (epay)
            var formData = new Dictionary<string, string>
            {
                { "amount", request.Amount.ToString("0.00") },
                { "tax_amount", request.TaxAmount.ToString("0.00") },
                { "total_amount", totalAmount.ToString("0.00") },
                { "transaction_uuid", transactionId },
                { "product_code", _esewaConfig.MerchantCode },
                { "product_service_charge", request.ServiceCharge.ToString("0.00") },
                { "product_delivery_charge", request.DeliveryCharge.ToString("0.00") },
                { "success_url", _esewaConfig.SuccessUrl },
                { "failure_url", _esewaConfig.FailureUrl },
                { "signed_field_names", "total_amount,transaction_uuid,product_code" }
            };

            // Generate signature
            var message = $"total_amount={totalAmount:0.00},transaction_uuid={transactionId},product_code={_esewaConfig.MerchantCode}";
            var signature = GenerateSignature(message);
            formData.Add("signature", signature);

            return await Task.FromResult(new PaymentResponse
            {
                Success = true,
                Message = "Payment initiated successfully",
                TransactionId = transactionId,
                PaymentUrl = _esewaConfig.PaymentUrl,
                FormData = formData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment");
            return new PaymentResponse
            {
                Success = false,
                Message = $"Error initiating payment: {ex.Message}"
            };
        }
    }

    public async Task<VerificationResponse> VerifyPayment(VerificationRequest request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            // Prepare verification URL with query parameters
            var verificationUrl = $"{_esewaConfig.VerificationUrl}?amt={request.Amount:0.00}&rid={request.RefId}&pid={request.ProductId}&scd={_esewaConfig.MerchantCode}";

            var response = await httpClient.GetAsync(verificationUrl);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"eSewa verification response: {content}");

            // Parse XML response from eSewa
            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(content))
            {
                try
                {
                    var xmlDoc = XDocument.Parse(content);
                    var responseCode = xmlDoc.Root?.Element("response_code")?.Value;

                    if (responseCode == "Success")
                    {
                        return new VerificationResponse
                        {
                            Success = true,
                            Message = "Payment verified successfully",
                            TransactionId = request.TransactionId,
                            RefId = request.RefId,
                            Amount = request.Amount,
                            Status = "Verified",
                            RawResponse = content
                        };
                    }
                }
                catch (Exception xmlEx)
                {
                    _logger.LogError(xmlEx, "Error parsing eSewa XML response");
                }
            }

            return new VerificationResponse
            {
                Success = false,
                Message = "Payment verification failed",
                TransactionId = request.TransactionId,
                Status = "Failed",
                RawResponse = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment");
            return new VerificationResponse
            {
                Success = false,
                Message = $"Error verifying payment: {ex.Message}",
                Status = "Error"
            };
        }
    }

    public string GenerateSignature(string message)
    {
        var secretKey = _esewaConfig.SecretKey;
        var encoding = new UTF8Encoding();
        var keyBytes = encoding.GetBytes(secretKey);
        var messageBytes = encoding.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
