using System.Text.Json.Serialization;

namespace wfc.referential.Domain.SupportAccountAggregate;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportAccountTypeEnum
{
    Individuel,
    Commun
}