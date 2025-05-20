using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ServiceAggregate;

public record ServiceId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public ServiceId(Guid value) => Value = value;

    public override string ToString() => Value.ToString();

    public static ServiceId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
            throw new Exception($"{nameof(ServiceId)} cannot be empty.");
        return new ServiceId(value);
    }

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}
