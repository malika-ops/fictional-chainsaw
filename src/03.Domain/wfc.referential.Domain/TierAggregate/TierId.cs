using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.TierAggregate;

public record TierId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public TierId(Guid value) => Value = value;

    public static TierId Of(Guid guid) =>
        guid == Guid.Empty ? throw new ArgumentException("TierId cannot be empty") : new TierId(guid);

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}
