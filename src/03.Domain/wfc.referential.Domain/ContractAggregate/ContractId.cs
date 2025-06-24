using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractAggregate;

public record ContractId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public ContractId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static ContractId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception("ContractId cannot be empty.");
        }

        return new ContractId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}