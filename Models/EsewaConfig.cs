namespace EsewaBackend.Models;

public class EsewaConfig
{
    public string MerchantCode { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string VerificationUrl { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string FailureUrl { get; set; } = string.Empty;
}
