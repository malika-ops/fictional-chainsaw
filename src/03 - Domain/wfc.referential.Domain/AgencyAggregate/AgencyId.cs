using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.AgencyAggregate;

public record AgencyId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public AgencyId(Guid value) => Value = value;

    public static AgencyId Of(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("AgencyId cannot be empty");
        return new AgencyId(value);
    }

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}