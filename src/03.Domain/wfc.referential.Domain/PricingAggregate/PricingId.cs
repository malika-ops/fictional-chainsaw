using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.PricingAggregate;

public record PricingId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public PricingId(Guid value) =>
        Value = value != Guid.Empty
            ? value
            : throw new ArgumentException("PricingId cannot be empty.");

    public static PricingId Of(Guid guid) => new(guid);

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}
