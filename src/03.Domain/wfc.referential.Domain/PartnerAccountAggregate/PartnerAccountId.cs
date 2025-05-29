using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAccountAggregate;

public record PartnerAccountId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public PartnerAccountId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static PartnerAccountId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("PartnerAccountId cannot be empty.");
        }

        return new PartnerAccountId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}