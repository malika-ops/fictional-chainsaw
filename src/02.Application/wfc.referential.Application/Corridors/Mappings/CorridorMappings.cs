using Mapster;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Corridors.Mappings;

public class CorridorMappings
{
    public static void Register()
    {

        TypeAdapterConfig<Corridor, GetAllCorridorsResponse>.NewConfig()
            .Map(dest => dest.CorridorId, src => src.Id!.Value);

        TypeAdapterConfig<CorridorId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<CountryId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<AgencyId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<CityId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<Guid?, CorridorId>
            .NewConfig()
            .MapWith(src => src.HasValue ? CorridorId.Of(src.Value) : null);
        TypeAdapterConfig<Guid?, AgencyId>
            .NewConfig()
            .MapWith(src => src.HasValue ? AgencyId.Of(src.Value) : null);
        TypeAdapterConfig<Guid?, CountryId>
            .NewConfig()
            .MapWith(src => src.HasValue ? CountryId.Of(src.Value) : null);
        TypeAdapterConfig<Guid?, CityId>
            .NewConfig()
            .MapWith(src => src.HasValue ? CityId.Of(src.Value) : null);

    }
}
