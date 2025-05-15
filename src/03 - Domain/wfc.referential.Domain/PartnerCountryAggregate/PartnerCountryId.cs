using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.PartnerCountryAggregate;

public record PartnerCountryId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public PartnerCountryId(Guid value) => Value = value;

    public static PartnerCountryId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{nameof(PartnerCountryId)} cannot be empty.", nameof(value));

        return new PartnerCountryId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
