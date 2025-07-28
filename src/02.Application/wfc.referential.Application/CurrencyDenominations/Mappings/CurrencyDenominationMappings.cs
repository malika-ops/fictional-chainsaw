using Mapster;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.Currencies.Mappings;

public class CurrencyDenominationMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CurrencyDenominationId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<CurrencyDenomination, GetCurrenciesResponse>
            .NewConfig()
            .Map(d => d.CurrencyDenominationId, s => s.Id.Value);
    }
}