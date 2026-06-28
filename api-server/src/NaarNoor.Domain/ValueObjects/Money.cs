namespace NaarNoor.Domain.ValueObjects;

/// <summary>
/// Money value object representing a monetary amount with currency.
/// This value object is immutable and implements equality comparison.
/// </summary>
public class Money : IEquatable<Money>
{
    /// <summary>
    /// The amount of money.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// The currency code (ISO 4217 format, e.g., "USD", "EUR").
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Creates a new Money value object with the specified amount and currency.
    /// </summary>
    /// <param name="amount">The monetary amount. Must be >= 0.</param>
    /// <param name="currency">The currency code. Cannot be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when amount is negative or currency is invalid.</exception>
    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty.", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters (ISO 4217 format).", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Determines whether the specified Money object is equal to the current Money object.
    /// Two Money objects are equal if they have the same amount and currency.
    /// </summary>
    public bool Equals(Money? other)
    {
        if (other is null)
            return false;

        return Amount == other.Amount && Currency == other.Currency;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Money object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Money money && Equals(money);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    /// <summary>
    /// Returns a string representation of the Money object in the format "{Amount} {Currency}".
    /// </summary>
    public override string ToString()
    {
        return $"{Amount:F2} {Currency}";
    }

    /// <summary>
    /// Determines whether two Money objects are equal.
    /// </summary>
    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Money objects are not equal.
    /// </summary>
    public static bool operator !=(Money? left, Money? right)
    {
        return !(left == right);
    }
}
