using Carter;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.API.Data;
using PaymentGateway.API.Models;
using PaymentGateway.API.Services;

namespace PaymentGateway.API.Endpoints;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/payments").WithTags("Payments");

        // Create Payment
        group.MapPost("/", async (
            [FromBody] CreatePaymentRequest request,
            [FromServices] IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            var result = await paymentService.CreatePaymentAsync(request, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("CreatePayment")
        .WithDescription("ایجاد درخواست پرداخت جدید")
        .Produces<PaymentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // Verify Payment
        group.MapPost("/verify", async (
            [FromBody] VerifyPaymentRequest request,
            [FromServices] IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            var result = await paymentService.VerifyPaymentAsync(request, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("VerifyPayment")
        .WithDescription("تأیید پرداخت")
        .Produces<VerifyPaymentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // Refund Payment
        group.MapPost("/{paymentId}/refund", async (
            string paymentId,
            [FromBody] RefundPaymentRequest request,
            [FromServices] IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            var result = await paymentService.RefundPaymentAsync(paymentId, request, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("RefundPayment")
        .WithDescription("استرداد پرداخت")
        .Produces<RefundPaymentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // Get Payment
        group.MapGet("/{paymentId}", async (
            string paymentId,
            [FromServices] IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            var payment = await paymentService.GetPaymentAsync(paymentId, cancellationToken);
            return payment != null ? Results.Ok(payment) : Results.NotFound();
        })
        .WithName("GetPayment")
        .WithDescription("دریافت اطلاعات پرداخت")
        .Produces<Payment>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


    }
}