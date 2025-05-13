using Mapster;
using wfc.referential.Application.Cities.Commands.CreateCity;
using wfc.referential.Application.Cities.Commands.DeleteCity;
using wfc.referential.Application.Cities.Commands.PatchCity;
using wfc.referential.Application.Cities.Commands.UpdateCity;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Cities.Queries.GetAllCities;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Mappings;

public class CityMappings
{

    public static void Register()
    {


        // Cities Mapping
        TypeAdapterConfig<CreateCityRequest, CreateCityCommand>.NewConfig();

        TypeAdapterConfig<City, GetAllCitiesResponse>.NewConfig()
            .Map(dest => dest.CityId, src => src.Id!.Value)
            .Map(dest => dest.RegionId, src => src.RegionId.Value);

        TypeAdapterConfig<GetAllCitiesRequest, GetAllCitiesQuery>
        .NewConfig();

        TypeAdapterConfig<PatchCityRequest, PatchCityCommand>
            .NewConfig()
            .IgnoreNullValues(true);
        
        TypeAdapterConfig<PatchCityCommand, City>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<DeleteCityRequest, DeleteCityCommand>.NewConfig();

        TypeAdapterConfig<UpdateCityRequest, UpdateCityCommand>.NewConfig();

    }
}
