using EsewaBackend.Models;

namespace EsewaBackend.Services;

public interface IEsewaService
{
    Task<PaymentResponse> InitiatePayment(PaymentRequest request);
    Task<VerificationResponse> VerifyPayment(VerificationRequest request);
}
