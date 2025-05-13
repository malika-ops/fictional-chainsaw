using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.IdentityDocumentAggregate;

public record IdentityDocumentId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public IdentityDocumentId(Guid value) => Value = value;

    public static IdentityDocumentId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(value));

        return new IdentityDocumentId(value);
    }

    public IEnumerable<object> GetEqualityComponents() { yield return Value; }
}
