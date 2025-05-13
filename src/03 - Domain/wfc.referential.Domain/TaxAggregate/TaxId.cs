using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TaxAggregate;

public record TaxId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public TaxId(Guid value) => Value = value;

    public override string ToString() => Value.ToString();
    public static TaxId Create() => new (Guid.NewGuid());

    public static TaxId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception($"{nameof(TaxId)} cannot be empty.");
        }
        return new TaxId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
