using Mapster;
using wfc.referential.Application.CountryServices.Dtos;
using wfc.referential.Domain.CountryServiceAggregate;

namespace wfc.referential.Application.CountryServices.Mappings;

public class CountryServiceMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CountryServiceId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<CountryService, GetCountryServicesResponse>
            .NewConfig()
            .Map(d => d.CountryServiceId, s => s.Id.Value)
            .Map(d => d.CountryId, s => s.CountryId.Value)
            .Map(d => d.ServiceId, s => s.ServiceId.Value);
    }
}