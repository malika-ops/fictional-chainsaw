using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.MonetaryZoneAggregate;

public record MonetaryZoneId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public MonetaryZoneId(Guid value) => Value = value;
    public override string ToString()
    {
        return Value.ToString();
    }
    public static MonetaryZoneId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception("MonetaryZoneId cannot be empty.");
        }

        return new MonetaryZoneId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
