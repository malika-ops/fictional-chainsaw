using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractDetailsAggregate;

public record ContractDetailsId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public ContractDetailsId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static ContractDetailsId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception("ContractDetailsId cannot be empty.");
        }

        return new ContractDetailsId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}