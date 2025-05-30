using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CountryIdentityDocAggregate;

public record CountryIdentityDocId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public CountryIdentityDocId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static CountryIdentityDocId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("CountryIdentityDocId cannot be empty.");
        }
        return new CountryIdentityDocId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}