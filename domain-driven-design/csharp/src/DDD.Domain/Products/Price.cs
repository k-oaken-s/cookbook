namespace DDD.Domain.Products;

using DDD.Domain.Common;
using System.Globalization;

public class Price : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Price Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Price amount cannot be negative", nameof(amount));
            
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));
            
        // ここではシンプルに3文字の通貨コードとします（例：USD, JPY, EUR）
        if (currency.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters", nameof(currency));
            
        return new Price(amount, currency);
    }
    
    public static Price CreateInDefaultCurrency(decimal amount)
    {
        return Create(amount, "USD");
    }
    
    public Price Add(Price price)
    {
        if (price.Currency != Currency)
            throw new ArgumentException("Cannot add prices with different currencies");
            
        return Create(Amount + price.Amount, Currency);
    }
    
    public Price Subtract(Price price)
    {
        if (price.Currency != Currency)
            throw new ArgumentException("Cannot subtract prices with different currencies");
            
        return Create(Amount - price.Amount, Currency);
    }
    
    public Price MultiplyBy(decimal multiplier)
    {
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