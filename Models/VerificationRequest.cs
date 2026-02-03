namespace EsewaBackend.Models;

public class VerificationRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public string TransactionUuid { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
