using Mapster;
using wfc.referential.Application.Corridors.Commands.PatchCorridor;
using wfc.referential.Application.Corridors.Commands.UpdateCorridor;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Corridors.Mappings;

public class CorridorMappings
{
    public static void Register()
    {

        TypeAdapterConfig<PatchCorridorCommand, Corridor>
            .NewConfig()
            .IgnoreNullValues(true);
        TypeAdapterConfig<UpdateCorridorCommand, Corridor>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<CountryId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<AgencyId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<CityId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

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
