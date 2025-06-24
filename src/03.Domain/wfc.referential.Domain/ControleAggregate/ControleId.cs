using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.ControleAggregate;

public record ControleId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public ControleId(Guid value) => Value = value;

    public static ControleId Of(Guid guid) =>
        guid == Guid.Empty ? throw new ArgumentException("ControleId cannot be empty") : new ControleId(guid);

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}