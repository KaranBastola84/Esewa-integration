using Microsoft.AspNetCore.Mvc;
using EsewaBackend.Models;
using EsewaBackend.Services;

namespace EsewaBackend.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IEsewaService _esewaService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IEsewaService esewaService,
        ILogger<PaymentController> logger)
    {
        _esewaService = esewaService;
        _logger = logger;
    }

    // ================= INITIATE PAYMENT =================
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _esewaService.InitiatePayment(request);

        if (!result.Success)
        {
            _logger.LogWarning("Payment initiation failed: {Message}", result.Message);
            return BadRequest(result);
        }

        return Ok(result);
    }

    // ================= SUCCESS CALLBACK =================
    [HttpGet("success")]
    public IActionResult PaymentSuccess(
        [FromQuery(Name = "transaction_uuid")] string transactionUuid,
        [FromQuery(Name = "status")] string status)
    {
        _logger.LogInformation(
            "Payment success callback received. Transaction: {Txn}, Status: {Status}",
            transactionUuid, status);

        // Frontend should call /verify after this

        return Ok(new
        {
            Message = "Payment success callback received",
            TransactionId = transactionUuid,
            Status = status
        });
    }

    // ================= FAILURE CALLBACK =================
    [HttpGet("failure")]
    public IActionResult PaymentFailure()
    {
        _logger.LogWarning("Payment failure callback received");

        return BadRequest(new
        {
            Message = "Payment failed or cancelled"
        });
    }

    // ================= VERIFY PAYMENT =================
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerificationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _esewaService.VerifyPayment(request);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Payment verification failed for {Txn}",
                request.TransactionUuid);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
