namespace EsewaBackend.Models;

public class EsewaConfig
{
    public string ProductCode { get; set; } = string.Empty; // EPAYTEST
    public string SecretKey { get; set; } = string.Empty;

    public string PaymentUrl { get; set; } = string.Empty;
    public string VerificationUrl { get; set; } = string.Empty;

    public string SuccessUrl { get; set; } = string.Empty;
    public string FailureUrl { get; set; } = string.Empty;

    public string SignedFieldNames { get; set; } =
        "total_amount,transaction_uuid,product_code";
}

