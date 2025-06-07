namespace Order.Domain.Exceptions;

public class OrderValidationException : OrderDomainException
{
    public string PropertyName { get; }

    public OrderValidationException(string propertyName, string message)
        : base(message)
    {
        PropertyName = propertyName;
    }
}