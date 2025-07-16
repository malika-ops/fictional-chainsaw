using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.ServiceAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlowDirection
{
    [EnumMember(Value = "Debit")]
    Debit,

    [EnumMember(Value = "Credit")]
    Credit,

    [EnumMember(Value = "None")]
    None,
}
