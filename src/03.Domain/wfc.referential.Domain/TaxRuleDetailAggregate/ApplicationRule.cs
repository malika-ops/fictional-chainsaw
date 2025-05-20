using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.TaxRuleDetailAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApplicationRule
{
    [EnumMember(Value = "Fees")]
    Fees,

    [EnumMember(Value = "Amount")]
    Amount,
}