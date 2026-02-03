namespace EsewaBackend.Models;

public class PaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public Dictionary<string, string>? FormData { get; set; }
}
