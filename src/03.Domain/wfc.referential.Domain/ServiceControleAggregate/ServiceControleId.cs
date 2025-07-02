using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.ServiceControleAggregate;

public record ServiceControleId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public ServiceControleId(Guid value) => Value = value;

    public static ServiceControleId Of(Guid guid) =>
        guid == Guid.Empty
            ? throw new ArgumentException("ServiceControleId cannot be empty")
            : new ServiceControleId(guid);

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}