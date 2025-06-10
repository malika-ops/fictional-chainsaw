using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CountryServiceAggregate;

public record CountryServiceId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public CountryServiceId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static CountryServiceId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("CountryServiceId cannot be empty.");
        }
        return new CountryServiceId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}