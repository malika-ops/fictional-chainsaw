using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ProductAggregate;

public record ProductId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public ProductId(Guid value) => Value = value;
    public override string ToString()
    {
        return Value.ToString();
    }
    public static ProductId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception($"{nameof(ProductId)} cannot be empty.");
        }

        return new ProductId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
