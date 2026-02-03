namespace EsewaBackend.Models;

public class VerificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string RefId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public enum EsewaPaymentStatus
    {
        COMPLETE,
        PENDING,
        FAILED
    }
    public string? RawResponse { get; set; }
}
