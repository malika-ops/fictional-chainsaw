using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AffiliateAggregate;

public record AffiliateId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public AffiliateId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static AffiliateId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception("AffiliateId cannot be empty.");
        }

        return new AffiliateId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}