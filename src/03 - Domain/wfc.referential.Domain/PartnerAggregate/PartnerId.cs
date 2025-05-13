using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAggregate;

public record PartnerId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public PartnerId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static PartnerId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception("PartnerId cannot be empty.");
        }

        return new PartnerId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}