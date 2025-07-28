using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyDenominationAggregate;

public record CurrencyDenominationId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public CurrencyDenominationId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static CurrencyDenominationId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("CurrencyDenominationId cannot be empty.");
        }
        return new CurrencyDenominationId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}