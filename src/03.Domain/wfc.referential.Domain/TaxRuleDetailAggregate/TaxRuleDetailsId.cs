using System.Text.Json.Serialization;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TaxRuleDetailAggregate;

public record TaxRuleDetailsId : IValueObject
{
    public Guid Value { get; }

    [JsonConstructor]
    public TaxRuleDetailsId(Guid value) => Value = value;

    public override string ToString() => Value.ToString();
    public static TaxRuleDetailsId Create() => new (Guid.NewGuid());

    public static TaxRuleDetailsId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new Exception($"{nameof(TaxRuleDetailsId)} cannot be empty.");
        }
        return new TaxRuleDetailsId(value);
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
