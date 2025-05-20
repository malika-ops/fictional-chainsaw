using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.RegionAggregate;

public record RegionId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public RegionId(Guid value) => Value = value;
    public override string ToString()
    {
        return Value.ToString();
    }
    public static RegionId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception($"{nameof(RegionId)} cannot be empty.");
        }

        return new RegionId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
