using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CityAggregate;

public record CityId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public CityId(Guid value) => Value = value;

    public override string ToString() => Value.ToString();
    public static CityId Create() => new CityId(Guid.NewGuid());

    public static CityId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception($"{nameof(CityId)} cannot be empty.");
        }
        return new CityId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

}
