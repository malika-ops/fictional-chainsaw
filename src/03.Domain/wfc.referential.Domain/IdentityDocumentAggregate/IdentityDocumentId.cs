using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.IdentityDocumentAggregate;

public record IdentityDocumentId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public IdentityDocumentId(Guid value) => Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static IdentityDocumentId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new BusinessException("IdentityDocumentId cannot be empty.");
        }
        return new IdentityDocumentId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}