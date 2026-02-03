using Microsoft.AspNetCore.Mvc;
using EsewaBackend.Models;
using EsewaBackend.Services;

namespace EsewaBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IEsewaService _esewaService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IEsewaService esewaService, ILogger<PaymentController> logger)
    {
        _esewaService = esewaService;
        _logger = logger;
    }

    /// <summary>
    /// Initiate a new payment with eSewa
    /// </summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _esewaService.InitiatePayment(request);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Verify payment after eSewa callback
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerificationRequest request)
    {
        if (string.IsNullOrEmpty(request.TransactionId))
        {
            return BadRequest(new { message = "Transaction ID is required" });
        }

        var result = await _esewaService.VerifyPayment(request);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// eSewa success callback endpoint
    /// </summary>
    [HttpGet("success")]
    public IActionResult PaymentSuccess(
        [FromQuery(Name = "oid")] string? orderId,
        [FromQuery(Name = "amt")] string? amount,
        [FromQuery(Name = "refId")] string? refId)
    {
        _logger.LogInformation($"Payment success callback: OrderId={orderId}, Amount={amount}, RefId={refId}");

        // Return a simple HTML page or redirect to frontend
        return Content($@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Payment Success</title>
                <style>
                    body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
                    .success {{ color: green; }}
                </style>
            </head>
            <body>
                <h1 class='success'>Payment Successful!</h1>
                <p>Order ID: {orderId}</p>
                <p>Amount: Rs. {amount}</p>
                <p>Reference ID: {refId}</p>
                <p>Please verify the payment to complete the transaction.</p>
            </body>
            </html>
        ", "text/html");
    }

    /// <summary>
    /// eSewa failure callback endpoint
    /// </summary>
    [HttpGet("failure")]
    public IActionResult PaymentFailure(
        [FromQuery(Name = "pid")] string? productId,
        [FromQuery(Name = "message")] string? message)
    {
        _logger.LogWarning($"Payment failure callback: ProductId={productId}, Message={message}");

        return Content($@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Payment Failed</title>
                <style>
                    body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
                    .failure {{ color: red; }}
                </style>
            </head>
            <body>
                <h1 class='failure'>Payment Failed</h1>
                <p>Product ID: {productId}</p>
                <p>Message: {message}</p>
                <p>Please try again or contact support.</p>
            </body>
            </html>
        ", "text/html");
    }
}
