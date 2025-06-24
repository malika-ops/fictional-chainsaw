using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TypeDefinitionAggregate;

public record TypeDefinitionId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public TypeDefinitionId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static TypeDefinitionId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("TypeDefinitionId cannot be empty.");
        }
        return new TypeDefinitionId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}