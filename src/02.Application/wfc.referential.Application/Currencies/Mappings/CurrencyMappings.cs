using Mapster;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Application.Currencies.Mappings;

public class CurrencyMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CurrencyId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<Currency, GetCurrenciesResponse>
            .NewConfig()
            .Map(d => d.CurrencyId, s => s.Id.Value);
    }
}