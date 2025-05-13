using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.AgencyTierAggregate;

public record AgencyTierId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public AgencyTierId(Guid value) => Value = value;

    public static AgencyTierId Of(Guid guid) =>
        guid == Guid.Empty ? throw new ArgumentException("AgencyTierId cannot be empty") : new AgencyTierId(guid);

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}