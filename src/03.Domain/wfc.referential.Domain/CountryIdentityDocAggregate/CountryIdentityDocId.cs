using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CountryIdentityDocAggregate;

public record CountryIdentityDocId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public CountryIdentityDocId(Guid value) => Value = value;

    public static CountryIdentityDocId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(value));

        return new CountryIdentityDocId(value);
    }

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}