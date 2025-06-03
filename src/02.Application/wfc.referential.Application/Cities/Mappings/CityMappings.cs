using Mapster;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Application.Cities.Mappings;

public class CityMappings
{

    public static void Register()
    {

        TypeAdapterConfig<City, GetAllCitiesResponse>.NewConfig()
            .Map(dest => dest.CityId, src => src.Id!.Value)
            .Map(dest => dest.RegionId, src => src.RegionId.Value);


        TypeAdapterConfig<CityId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);
    }
}
