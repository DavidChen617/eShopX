using Domain.Exceptions;

namespace Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money Of(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentInvalidException("Amount cannot be negative.");
        return new Money(amount);
    }

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
    public static Money operator *(Money money, int quantity) => new(money.Amount * quantity);
}
