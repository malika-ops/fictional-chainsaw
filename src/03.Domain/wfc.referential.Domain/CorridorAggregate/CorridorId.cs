using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CorridorAggregate;

public record CorridorId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public CorridorId(Guid value) => Value = value;
    public override string ToString()
    {
        return Value.ToString();
    }
    public static CorridorId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception($"{nameof(CorridorId)} cannot be empty.");
        }

        return new CorridorId(value);
    }
    public static CorridorId Create() => new CorridorId(Guid.NewGuid());
    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
