using Mapster;
using wfc.referential.Application.Cities.Commands.PatchCity;
using wfc.referential.Application.Corridors.Commands.CreateCorridor;
using wfc.referential.Application.Corridors.Commands.PatchCorridor;
using wfc.referential.Application.Corridors.Commands.UpdateCorridor;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Corridors.Queries.GetAllCorridors;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Corridors.Mappings;

public class CorridorMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CreateCorridorRequest, CreateCorridorCommand>.NewConfig()
            .Map(dest => dest.SourceCountryId, src => CountryId.Of(src.SourceCountryId))
            .Map(dest => dest.DestinationCountryId, src => CountryId.Of(src.DestinationCountryId))
            .Map(dest => dest.SourceCityId, src => src.SourceCityId.HasValue ? CityId.Of(src.SourceCityId.Value) : null)
            .Map(dest => dest.DestinationCityId, src => src.DestinationCityId.HasValue ? CityId.Of(src.DestinationCityId.Value) : null)
            .Map(dest => dest.SourceAgencyId, src => src.SourceAgencyId.HasValue ? AgencyId.Of(src.SourceAgencyId.Value) : null)
            .Map(dest => dest.DestinationAgencyId, src => src.DestinationAgencyId.HasValue ? AgencyId.Of(src.DestinationAgencyId.Value) : null);

        TypeAdapterConfig<GetAllCorridorsRequest, GetAllCorridorsQuery>
            .NewConfig()
            .Map(dest => dest.SourceCountryId,
                src => src.SourceCountryId.HasValue ? CountryId.Of(src.SourceCountryId.Value) : null)
            .Map(dest => dest.DestinationCountryId,
                src => src.DestinationCountryId.HasValue ? CountryId.Of(src.DestinationCountryId.Value) : null)
            .Map(dest => dest.SourceCityId,
                src => src.SourceCityId.HasValue ? CityId.Of(src.SourceCityId.Value) : null)
            .Map(dest => dest.DestinationCityId,
                src => src.DestinationCityId.HasValue ? CityId.Of(src.DestinationCityId.Value) : null)
            .Map(dest => dest.SourceAgencyId,
                src => src.SourceAgencyId.HasValue ? AgencyId.Of(src.SourceAgencyId.Value) : null)
            .Map(dest => dest.DestinationAgencyId,
                src => src.DestinationAgencyId.HasValue ? AgencyId.Of(src.DestinationAgencyId.Value) : null)
            .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
            .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<PatchCorridorCommand, Corridor>
            .NewConfig()
            .IgnoreNullValues(true);
        TypeAdapterConfig<UpdateCorridorCommand, Corridor>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<Corridor, GetAllCorridorsResponse>
            .NewConfig()
            .ConstructUsing(corridor => new GetAllCorridorsResponse
            {
                CorridorId = corridor.Id!.Value,
                SourceCountryId = corridor.SourceCountryId != null ? corridor.SourceCountryId.Value : null,
                DestinationCountryId = corridor.DestinationCountryId != null ? corridor.DestinationCountryId.Value : null,
                SourceCityId = corridor.SourceCityId != null ? corridor.SourceCityId.Value : null,
                DestinationCityId = corridor.DestinationCityId != null ? corridor.DestinationCityId.Value : null,
                SourceAgencyId = corridor.SourceAgencyId != null ? corridor.SourceAgencyId.Value : null,
                DestinationAgencyId = corridor.DestinationAgencyId != null ? corridor.DestinationAgencyId.Value : null,
                IsEnabled = corridor.IsEnabled
            });

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
