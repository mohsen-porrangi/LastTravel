using Carter;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.API.Models;
using PaymentGateway.API.Services;

namespace PaymentGateway.API.Endpoints
{
    public class CallbackEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Payment Callback
            app.MapGet("/callback", async (
                HttpContext context,
                [FromServices] IPaymentService paymentService,
                [FromQuery] string? authority,
                [FromQuery] string? status,
                [FromQuery] decimal? amount,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrEmpty(authority) || !amount.HasValue)
                {
                    return Results.BadRequest("پارامترهای کالبک نامعتبر است");
                }

                var verifyRequest = new VerifyPaymentRequest
                {
                    Authority = authority,
                    Status = status ?? "NOK",
                    Amount = amount.Value
                };

                var result = await paymentService.VerifyPaymentAsync(verifyRequest, cancellationToken);

                // Redirect based on result
                var redirectUrl = result.IsSuccessful
                    ? $"/payment/success?referenceId={result.ReferenceId}&amount={result.Amount}"
                    : $"/payment/failure?error={Uri.EscapeDataString(result.ErrorMessage ?? "خطای نامشخص")}";

                return Results.Redirect(redirectUrl);
            })
            .WithName("PaymentCallback")
            .WithDescription("کالبک پرداخت از درگاه")
            .Produces(StatusCodes.Status302Found)
            .AllowAnonymous();
        }
    }
}
