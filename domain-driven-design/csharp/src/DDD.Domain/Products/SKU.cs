namespace DDD.Domain.Products;

using DDD.Domain.Common;
using System.Text.RegularExpressions;

public class SKU : ValueObject
{
    public string Value { get; }
    
    private SKU(string value)
    {
        Value = value;
    }
    
    public static SKU Create(string sku)
    {
        // SKUの形式は例えばXXX-YYYY-ZZZZというフォーマットとします
        var pattern = @"^[A-Z]{3}-\d{4}-[A-Z0-9]{4}$";
        
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));
            
        if (!Regex.IsMatch(sku, pattern))
            throw new ArgumentException("SKU must be in XXX-YYYY-ZZZZ format", nameof(sku));
            
        return new SKU(sku);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
}