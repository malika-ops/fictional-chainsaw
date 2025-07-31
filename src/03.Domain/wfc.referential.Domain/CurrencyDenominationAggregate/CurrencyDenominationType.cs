using System.Text.Json.Serialization;

namespace wfc.referential.Domain.CurrencyDenominationAggregate;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CurrencyDenominationType
{
    Coin,
    Banknote
}