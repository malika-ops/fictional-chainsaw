using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SupportAccountAggregate;

public record SupportAccountId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public SupportAccountId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static SupportAccountId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("SupportAccountId cannot be empty.");
        }
        return new SupportAccountId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}