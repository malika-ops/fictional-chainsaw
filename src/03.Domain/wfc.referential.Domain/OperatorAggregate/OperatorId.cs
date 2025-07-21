using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.OperatorAggregate;

public record OperatorId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public OperatorId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static OperatorId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception("OperatorId cannot be empty.");
        }

        return new OperatorId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}