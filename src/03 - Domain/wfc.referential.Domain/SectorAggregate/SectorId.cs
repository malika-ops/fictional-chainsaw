using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate;

public record SectorId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public SectorId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static SectorId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception("SectorId cannot be empty.");
        }

        return new SectorId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}