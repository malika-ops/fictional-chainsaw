using BuildingBlocks.Core.Abstraction.Domain;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.ParamTypeAggregate;

public record ParamTypeId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public ParamTypeId(Guid value) => Value = value;
    public override string ToString()
    {
        return Value.ToString();
    }
    public static ParamTypeId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception("ParamTypeId cannot be empty.");
        }

        return new ParamTypeId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
