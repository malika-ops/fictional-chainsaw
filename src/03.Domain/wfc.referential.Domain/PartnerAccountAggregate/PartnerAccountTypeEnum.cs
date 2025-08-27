using System.Text.Json.Serialization;

namespace wfc.referential.Domain.PartnerAccountAggregate;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PartnerAccountTypeEnum
{
    Activité,
    Commission
}