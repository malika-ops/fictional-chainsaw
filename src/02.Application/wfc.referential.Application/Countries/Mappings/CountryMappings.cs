using Mapster;
using wfc.referential.Application.Countries.Commands.CreateCountry;
using wfc.referential.Application.Countries.Commands.DeleteCountry;
using wfc.referential.Application.Countries.Commands.PatchCountry;
using wfc.referential.Application.Countries.Commands.UpdateCountry;
using wfc.referential.Application.Countries.Dtos;
using wfc.referential.Application.Countries.Queries.GetAllCounties;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Countries.Mappings;

public class CountryMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CreateCountryRequest, CreateCountryCommand>
                .NewConfig();

        TypeAdapterConfig<UpdateCountryRequest, UpdateCountryCommand>
            .NewConfig();

        TypeAdapterConfig<DeleteCountryRequest, DeleteCountryCommand>
            .NewConfig();


        TypeAdapterConfig<GetAllCountriesRequest, GetAllCountriesQuery>
            .NewConfig();

        TypeAdapterConfig<Country, GetCountriesResponce>
            .NewConfig()
            .Map(dest => dest.CountryId, src => src.Id.Value)
            .Map(dest => dest.MonetaryZoneId, src => src.MonetaryZoneId.Value);

        TypeAdapterConfig<List<Country>, List<GetCountriesResponce>>.NewConfig()
            .MapWith(src => src.Select(x => x.Adapt<GetCountriesResponce>()).ToList());

        // Request -> Command (ignore nulls)
        TypeAdapterConfig<PatchCountryRequest, PatchCountryCommand>
            .NewConfig()
            .IgnoreNullValues(true);

        // Command -> Aggregate (only non‑null props overwrite)
        TypeAdapterConfig<PatchCountryCommand, Country>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<CountryId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

    }
}
