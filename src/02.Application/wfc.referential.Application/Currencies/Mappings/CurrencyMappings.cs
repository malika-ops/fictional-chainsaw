using Mapster;
using wfc.referential.Application.Currencies.Commands.CreateCurrency;
using wfc.referential.Application.Currencies.Commands.DeleteCurrency;
using wfc.referential.Application.Currencies.Commands.PatchCurrency;
using wfc.referential.Application.Currencies.Commands.UpdateCurrency;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Currencies.Queries;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Application.Currencies.Mappings;

public class CurrencyMappings
{
    public static void Register()
    {

        TypeAdapterConfig<Currency, CurrencyResponse>
            .NewConfig()
            .Map(dest => dest.CountriesCount, src => src.Countries.Count);

        TypeAdapterConfig<CurrencyId, Guid>
            .NewConfig()
            .Map(dest => dest, src => src.Value);

     

        TypeAdapterConfig<PatchCurrencyCommand, Currency>
            .NewConfig()
            .IgnoreNullValues(true);
    }
}