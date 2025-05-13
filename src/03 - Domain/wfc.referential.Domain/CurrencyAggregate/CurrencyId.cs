using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyAggregate;

public record CurrencyId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public CurrencyId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static CurrencyId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("CurrencyId cannot be empty.");
        }
        return new CurrencyId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}