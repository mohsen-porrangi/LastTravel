using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Contracts;

namespace WalletPayment.API.Endpoints.BankAccounts;

/// <summary>
/// Add bank account endpoint
/// </summary>
public class AddBankAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/bank-accounts", AddBankAccountAsync)
            .WithName("AddBankAccount")
            .WithTags("Bank Accounts")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Add bank account";
                operation.Description = "Add new bank account to user's wallet";
                return operation;
            });
    }

    public record AddBankAccountRequest(
        string BankName,
        string AccountNumber,
        string? CardNumber = null,
        string? ShabaNumber = null,
        string? AccountHolderName = null
    );

    [Authorize]
    private static async Task<IResult> AddBankAccountAsync(
        AddBankAccountRequest request,
        ICurrentUserService currentUserService,
        WalletPayment.Domain.Common.Contracts.IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var wallet = await unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
            userId,
            includeCurrencyAccounts: false,
            includeBankAccounts: true,
            cancellationToken: cancellationToken);

        if (wallet == null)
        {
            return Results.NotFound(new { error = "کیف پول یافت نشد" });
        }

        var bankAccount = wallet.AddBankAccount(
            request.BankName,
            request.AccountNumber,
            request.CardNumber,
            request.ShabaNumber,
            request.AccountHolderName);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/bank-accounts/{bankAccount.Id}", new
        {
            id = bankAccount.Id,
            bankName = bankAccount.BankName,
            accountNumber = bankAccount.GetMaskedAccountNumber(),
            cardNumber = bankAccount.GetMaskedCardNumber(),
            shabaNumber = bankAccount.ShabaNumber,
            accountHolderName = bankAccount.AccountHolderName,
            isVerified = bankAccount.IsVerified,
            isDefault = bankAccount.IsDefault,
            isActive = bankAccount.IsActive,
            createdAt = bankAccount.CreatedAt
        });
    }
}