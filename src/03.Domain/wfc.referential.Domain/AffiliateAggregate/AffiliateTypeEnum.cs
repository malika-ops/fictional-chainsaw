using System.Text.Json.Serialization;

namespace wfc.referential.Domain.AffiliateAggregate;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AffiliateTypeEnum
{
    Wafacash,
    Paycash
}