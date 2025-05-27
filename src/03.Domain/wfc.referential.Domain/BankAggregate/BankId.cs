using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.BankAggregate;

public record BankId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public BankId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static BankId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("BankId cannot be empty.");
        }
        return new BankId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}