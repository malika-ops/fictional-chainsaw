using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace wfc.referential.Domain.CurrencyDenominationAggregate;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CurrencyDenominationType
{
    [EnumMember(Value = "Coin")]
    Coin,
    [EnumMember(Value = "Banknote")]
    Banknote
}