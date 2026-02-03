namespace EsewaBackend.Models;

public class VerificationRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RefId { get; set; } = string.Empty;
}
