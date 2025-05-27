using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate;

public record SectorId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public SectorId(Guid value) => Value = value;

    public static SectorId Of(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("SectorId cannot be empty");
        return new SectorId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}