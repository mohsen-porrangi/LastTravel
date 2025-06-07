// Application/Features/Transactions/IntegratedPurchase/IntegratedPurchaseHandler.cs
using BuildingBlocks.CQRS;
using WalletPayment.Application.Common.Interfaces;
using WalletPayment.Domain.Aggregates.WalletAggregate;
using WalletPayment.Domain.Common;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;
using FluentValidation;
using WalletPayment.Domain.Aggregates.TransactionAggregate;

namespace WalletPayment.Application.Features.Transactions.IntegratedPurchase;

/// <summary>
/// Complete Integrated Purchase Handler - Fixed Implementation
/// ✅ Handles three scenarios:
/// 1. Full wallet payment (موجودی کافی)
/// 2. Mixed payment (موجودی + درگاه)
/// 3. Full gateway payment (بدون موجودی)
/// </summary>
public class IntegratedPurchaseHandler : ICommandHandler<IntegratedPurchaseCommand, IntegratedPurchaseResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayClient _paymentGateway;

    public IntegratedPurchaseHandler(
        IUnitOfWork unitOfWork,
        IPaymentGatewayClient paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<IntegratedPurchaseResult> Handle(
        IntegratedPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet with necessary includes
        var wallet = await _unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
            request.UserId,
            includeCurrencyAccounts: true,
            includeBankAccounts: false,
            includeCredits: request.UseCredit,
            cancellationToken: cancellationToken);

        if (wallet == null)
        {
            throw new WalletNotFoundException(request.UserId);
        }

        if (!wallet.IsActive)
        {
            return new IntegratedPurchaseResult
            {
                IsSuccessful = false,
                ErrorMessage = DomainErrors.GetMessage(DomainErrors.Wallet.Inactive)
            };
        }

        var purchaseAmount = Money.Create(request.TotalAmount, request.Currency);

        // Validate purchase amount
        if (!BusinessRules.Amounts.IsValidTransactionAmount(purchaseAmount))
        {
            return new IntegratedPurchaseResult
            {
                IsSuccessful = false,
                ErrorMessage = "مبلغ خرید نامعتبر است"
            };
        }

        // Handle B2B credit purchase (future feature)
        if (request.UseCredit)
        {
            return await HandleCreditPurchaseAsync(wallet, purchaseAmount, request, cancellationToken);
        }

        // Get or create currency account
        var account = wallet.GetCurrencyAccount(request.Currency);
        if (account == null)
        {
            account = wallet.CreateCurrencyAccount(request.Currency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var currentBalance = account.Balance.Value;

        // 🎯 Main Business Logic: Determine purchase strategy
        return currentBalance switch
        {
            var balance when balance >= request.TotalAmount =>
                await ProcessFullWalletPurchaseAsync(account, purchaseAmount, request, cancellationToken),

            var balance when balance <= 0 =>
                await ProcessFullPaymentPurchaseAsync(purchaseAmount, request, cancellationToken),

            _ => await ProcessMixedPurchaseAsync(account, purchaseAmount, request, cancellationToken)
        };
    }

    /// <summary>
    /// Scenario 1: Full wallet payment (کل پول از کیف پول)
    /// </summary>
    private async Task<IntegratedPurchaseResult> ProcessFullWalletPurchaseAsync(
        CurrencyAccount account,
        Money purchaseAmount,
        IntegratedPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Create purchase transaction
            var purchaseTransaction = account.CreatePurchaseTransaction(
                purchaseAmount,
                $"خرید {request.Description}",
                request.OrderId);

            // Process the purchase (deduct from balance)
            account.ProcessPurchase(purchaseTransaction);

            // Save transaction
            await _unitOfWork.Transactions.AddAsync(purchaseTransaction, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new IntegratedPurchaseResult
            {
                IsSuccessful = true,
                PurchaseType = PurchaseType.FullWallet,
                TotalAmount = request.TotalAmount,
                WalletBalance = account.Balance.Value,
                RequiredPayment = 0,
                PurchaseTransactionId = purchaseTransaction.Id,
                ProcessedAt = DateTime.UtcNow
            };
        }, cancellationToken);
    }

    /// <summary>
    /// Scenario 2: Full payment gateway (کل پول از درگاه)
    /// </summary>
    private async Task<IntegratedPurchaseResult> ProcessFullPaymentPurchaseAsync(
        Money purchaseAmount,
        IntegratedPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        // Create payment request for full amount
        var paymentResult = await _paymentGateway.CreatePaymentAsync(
            purchaseAmount,
            $"خرید {request.Description} - سفارش {request.OrderId}",
            request.CallbackUrl,
            request.OrderId,
            cancellationToken: cancellationToken);

        if (!paymentResult.IsSuccessful)
        {
            return new IntegratedPurchaseResult
            {
                IsSuccessful = false,
                ErrorMessage = paymentResult.ErrorMessage ?? "خطا در ایجاد درخواست پرداخت"
            };
        }

        return new IntegratedPurchaseResult
        {
            IsSuccessful = true,
            PurchaseType = PurchaseType.FullPayment,
            TotalAmount = request.TotalAmount,
            WalletBalance = 0,
            RequiredPayment = request.TotalAmount,
            PaymentUrl = paymentResult.PaymentUrl,
            Authority = paymentResult.Authority
        };
    }

    /// <summary>
    /// Scenario 3: Mixed purchase (بخشی کیف پول + بخشی درگاه)
    /// 🎯 Core Business Logic: Use available wallet + gateway for remainder
    /// </summary>
    private async Task<IntegratedPurchaseResult> ProcessMixedPurchaseAsync(
        CurrencyAccount account,
        Money purchaseAmount,
        IntegratedPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        var walletAmount = account.Balance.Value;
        var paymentAmount = request.TotalAmount - walletAmount;

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Step 1: Use wallet balance
            var walletPurchaseAmount = Money.Create(walletAmount, request.Currency);
            var walletTransaction = account.CreatePurchaseTransaction(
                walletPurchaseAmount,
                $"خرید جزئی از کیف پول - {request.Description}",
                request.OrderId);

            // Process wallet purchase
            account.ProcessPurchase(walletTransaction);
            await _unitOfWork.Transactions.AddAsync(walletTransaction, ct);

            // Step 2: Create payment request for remaining amount
            var paymentMoney = Money.Create(paymentAmount, request.Currency);
            var paymentResult = await _paymentGateway.CreatePaymentAsync(
                paymentMoney,
                $"تکمیل خرید {request.Description} - سفارش {request.OrderId}",
                request.CallbackUrl,
                request.OrderId,
                cancellationToken: ct);

            if (!paymentResult.IsSuccessful)
            {
                // Transaction will rollback automatically
                return new IntegratedPurchaseResult
                {
                    IsSuccessful = false,
                    ErrorMessage = paymentResult.ErrorMessage ?? "خطا در ایجاد درخواست پرداخت"
                };
            }

            // Save wallet transaction
            await _unitOfWork.SaveChangesAsync(ct);

            return new IntegratedPurchaseResult
            {
                IsSuccessful = true,
                PurchaseType = PurchaseType.Mixed,
                TotalAmount = request.TotalAmount,
                WalletBalance = account.Balance.Value, // Should be 0 now
                RequiredPayment = paymentAmount,
                PurchaseTransactionId = walletTransaction.Id,
                PaymentUrl = paymentResult.PaymentUrl,
                Authority = paymentResult.Authority
            };
        }, cancellationToken);
    }

    /// <summary>
    /// B2B Credit Purchase (Future Implementation)
    /// </summary>
    private async Task<IntegratedPurchaseResult> HandleCreditPurchaseAsync(
        Wallet wallet,
        Money purchaseAmount,
        IntegratedPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        var activeCredit = wallet.GetActiveCredit();
        if (activeCredit == null)
        {
            return new IntegratedPurchaseResult
            {
                IsSuccessful = false,
                ErrorMessage = "اعتبار فعالی برای این کاربر وجود ندارد"
            };
        }

        if (!activeCredit.CanUseCredit(purchaseAmount))
        {
            return new IntegratedPurchaseResult
            {
                IsSuccessful = false,
                ErrorMessage = $"اعتبار کافی نیست. اعتبار موجود: {activeCredit.AvailableCredit.Value:N0}"
            };
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Use credit
            activeCredit.UseCredit(purchaseAmount);

            // Get default account (IRR) for transaction recording
            var defaultAccount = wallet.GetDefaultAccount();
            if (defaultAccount == null)
            {
                defaultAccount = wallet.CreateCurrencyAccount(CurrencyCode.IRR);
            }

            // Create credit transaction
            var creditTransaction = Transaction.CreateCreditTransaction(
                wallet.Id,
                defaultAccount.Id,
                wallet.UserId,
                purchaseAmount,
                $"خرید اعتباری - {request.Description}",
                activeCredit.DueDate,
                request.OrderId);

            // Mark as completed immediately (credit transactions are instant)
            creditTransaction.MarkAsCompleted();

            await _unitOfWork.Transactions.AddAsync(creditTransaction, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new IntegratedPurchaseResult
            {
                IsSuccessful = true,
                PurchaseType = PurchaseType.Credit,
                TotalAmount = request.TotalAmount,
                WalletBalance = defaultAccount.Balance.Value,
                RequiredPayment = 0,
                PurchaseTransactionId = creditTransaction.Id,
                ProcessedAt = DateTime.UtcNow
            };
        }, cancellationToken);
    }
}

