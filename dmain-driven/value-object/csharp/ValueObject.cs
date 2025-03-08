// domain-driven/value-object/csharp/ValueObject.cs

using System;
using System.Collections.Generic;
using System.Linq;

namespace DesignPatternsCookbook.DomainDriven.ValueObject
{
    // 値オブジェクトの基底クラス
    public abstract class ValueObject
    {
        // 等価性の比較に使用するコンポーネントを取得
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;
            
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        public static bool operator ==(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return !(left == right);
        }
    }

    // 値オブジェクトの例：お金
    public class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money() { } // EF Core用

        public Money(decimal amount, string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be empty", nameof(currency));

            Amount = amount;
            Currency = currency;
        }

        // ファクトリメソッド
        public static Money FromUsd(decimal amount) => new Money(amount, "USD");
        public static Money FromEur(decimal amount) => new Money(amount, "EUR");
        public static Money FromJpy(decimal amount) => new Money(amount, "JPY");
        public static Money Zero(string currency) => new Money(0, currency);

        // 値オブジェクトの操作
        public Money Add(Money money)
        {
            if (Currency != money.Currency)
                throw new InvalidOperationException($"Cannot add Money with different currencies: {Currency} and {money.Currency}");

            return new Money(Amount + money.Amount, Currency);
        }

        public Money Subtract(Money money)
        {
            if (Currency != money.Currency)
                throw new InvalidOperationException($"Cannot subtract Money with different currencies: {Currency} and {money.Currency}");

            return new Money(Amount - money.Amount, Currency);
        }

        public Money Multiply(decimal multiplier)
        {
            return new Money(Amount * multiplier, Currency);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public override string ToString()
        {
            return $"{Amount} {Currency}";
        }
    }

    // 値オブジェクトの例：住所
    public class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string Country { get; }
        public string ZipCode { get; }

        private Address() { } // EF Core用

        public Address(string street, string city, string state, string country, string zipCode)
        {
            Street = street ?? throw new ArgumentNullException(nameof(street));
            City = city ?? throw new ArgumentNullException(nameof(city));
            State = state ?? throw new ArgumentNullException(nameof(state));
            Country = country ?? throw new ArgumentNullException(nameof(country));
            ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return State;
            yield return Country;
            yield return ZipCode;
        }

        public override string ToString()
        {
            return $"{Street}, {City}, {State} {ZipCode}, {Country}";
        }
    }

    // 値オブジェクトの使用例
    public class Order
    {
        public Guid Id { get; private set; }
        public string CustomerName { get; private set; }
        public Address ShippingAddress { get; private set; }
        public Money TotalAmount { get; private set; }

        public Order(string customerName, Address shippingAddress, Money totalAmount)
        {
            Id = Guid.NewGuid();
            CustomerName = customerName;
            ShippingAddress = shippingAddress;
            TotalAmount = totalAmount;
        }

        public void UpdateShippingAddress(Address newAddress)
        {
            ShippingAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        }

        public void AddDiscount(Money discount)
        {
            if (discount.Amount < 0)
                throw new ArgumentException("Discount cannot be negative", nameof(discount));

            TotalAmount = TotalAmount.Subtract(discount);
        }
    }

    // 使用例
    public class ValueObjectDemo
    {
        public static void Run()
        {
            // Money値オブジェクトの作成と操作
            var price = Money.FromUsd(100.00m);
            var tax = price.Multiply(0.08m);
            var total = price.Add(tax);

            Console.WriteLine($"Price: {price}");
            Console.WriteLine($"Tax: {tax}");
            Console.WriteLine($"Total: {total}");

            // 等価性の確認
            var money1 = new Money(100.00m, "USD");
            var money2 = new Money(100.00m, "USD");
            var money3 = new Money(100.00m, "EUR");

            Console.WriteLine($"money1 == money2: {money1 == money2}"); // True
            Console.WriteLine($"money1 == money3: {money1 == money3}"); // False

            // Address値オブジェクトの作成
            var address1 = new Address("123 Main St", "Anytown", "NY", "USA", "12345");
            var address2 = new Address("123 Main St", "Anytown", "NY", "USA", "12345");

            Console.WriteLine($"address1 == address2: {address1 == address2}"); // True

            // 注文での使用
            var order = new Order("John Doe", address1, price);
            
            // 割引の適用
            var discount = Money.FromUsd(10.00m);
            order.AddDiscount(discount);
            
            // 住所の変更
            var newAddress = new Address("456 Elm St", "Othertown", "CA", "USA", "67890");
            order.UpdateShippingAddress(newAddress);
        }
    }
}