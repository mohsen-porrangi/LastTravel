using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Services;

public class OrderPricingService
{
    private const decimal DomesticFlightTaxRate = 0.09m; // 9%
    private const decimal InternationalFlightTaxRate = 0.12m; // 12%
    private const decimal TrainTaxRate = 0.09m; // 9%
    private const decimal ServiceFeeRate = 0.02m; // 2%

    public Money CalculateTotalPrice(ServiceType serviceType, Money basePrice, AgeGroup ageGroup)
    {
        var discountedPrice = ApplyAgeDiscount(basePrice, ageGroup);
        var tax = CalculateTax(serviceType, discountedPrice);
        var fee = CalculateFee(discountedPrice);

        return discountedPrice.Add(tax).Add(fee);
    }

    private Money ApplyAgeDiscount(Money basePrice, AgeGroup ageGroup)
    {
        var discountRate = ageGroup switch
        {
            AgeGroup.Child => 0.5m, // 50% discount
            AgeGroup.Infant => 0.9m, // 90% discount
            _ => 0m // No discount for adults
        };

        return basePrice.Multiply(1 - discountRate);
    }

    private Money CalculateTax(ServiceType serviceType, Money price)
    {
        var taxRate = serviceType switch
        {
            ServiceType.DomesticFlight => DomesticFlightTaxRate,
            ServiceType.InternationalFlight => InternationalFlightTaxRate,
            ServiceType.Train => TrainTaxRate,
            _ => 0m
        };

        return price.Multiply(taxRate);
    }

    private Money CalculateFee(Money price)
    {
        return price.Multiply(ServiceFeeRate);
    }
}