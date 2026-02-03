using System.ComponentModel.DataAnnotations;

namespace EsewaBackend.Models;

public class PaymentRequest
{
    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string ProductId { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public decimal TaxAmount { get; set; } = 0;

    public decimal ServiceCharge { get; set; } = 0;

    public decimal DeliveryCharge { get; set; } = 0;

    public string CustomerEmail { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;
}
