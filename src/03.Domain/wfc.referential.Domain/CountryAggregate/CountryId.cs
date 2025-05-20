using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.Countries;

public record CountryId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public CountryId(Guid value) => Value = value;
    public override string ToString()
    {
        return Value.ToString();
    }
    public static CountryId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception($"{nameof(CountryId)} cannot be empty.");
        }

        return new CountryId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
