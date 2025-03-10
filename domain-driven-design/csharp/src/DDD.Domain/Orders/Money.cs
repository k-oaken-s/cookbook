namespace DDD.Domain.Orders;

using DDD.Domain.Common;
using System.Globalization;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Money Create(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));
            
        // ここではシンプルに3文字の通貨コードとします（例：USD, JPY, EUR）
        if (currency.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters", nameof(currency));
            
        return new Money(amount, currency);
    }
    
    public static Money Zero(string currency)
    {
        return Create(0, currency);
    }
    
    public static Money FromString(string moneyString, string currency)
    {
        if (!decimal.TryParse(moneyString, NumberStyles.Currency, CultureInfo.InvariantCulture, out var amount))
            throw new ArgumentException("Invalid money format", nameof(moneyString));
            
        return Create(amount, currency);
    }
    
    public Money Add(Money money)
    {
        if (money.Currency != Currency)
            throw new ArgumentException("Cannot add money with different currencies");
            
        return Create(Amount + money.Amount, Currency);
    }
    
    public Money Subtract(Money money)
    {
        if (money.Currency != Currency)
            throw new ArgumentException("Cannot subtract money with different currencies");
            
        if (Amount < money.Amount)
            throw new InvalidOperationException("Cannot have negative money amount");
            
        return Create(Amount - money.Amount, Currency);
    }
    
    public Money MultiplyBy(decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));
            
        return Create(Amount * multiplier, Currency);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public override string ToString()
    {
        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
        nfi.CurrencySymbol = Currency;
        return Amount.ToString("C", nfi);
    }
}
