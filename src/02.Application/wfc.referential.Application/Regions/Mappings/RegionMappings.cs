using Mapster;
using wfc.referential.Application.RegionManagement.Dtos;
using wfc.referential.Application.RegionManagement.Queries.GetAllRegions;
using wfc.referential.Application.Regions.Commands.CreateRegion;
using wfc.referential.Application.Regions.Commands.DeleteRegion;
using wfc.referential.Application.Regions.Commands.PatchRegion;
using wfc.referential.Application.Regions.Dtos;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Regions.Mappings;

public class RegionMappings
{
    public static void Register()
    {
        // Regions
        //TypeAdapterConfig<GetAllRegionsRequest, GetAllRegionsQuery>
        //.NewConfig()
        //.Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
        //.Map(dest => dest.PageSize, src => src.PageSize ?? 10)
        //.Map(dest => dest.CountryId, src => src.CountryId.HasValue ? new CountryId(src.CountryId.Value) : null)
        //.Map(dest => dest.IsEnabled, src => src.IsEnabled);

        //TypeAdapterConfig<Region, GetAllRegionsResponse>.NewConfig()
        //    .Map(dest => dest.RegionId, src => src.Id!.Value);

        //TypeAdapterConfig<CreateRegionRequest, CreateRegionCommand>.NewConfig()
        //    .Map(dest => dest.CountryId, src => CountryId.Of(src.CountryId));

        //TypeAdapterConfig<PatchRegionRequest, PatchRegionCommand>
        //    .NewConfig()
        //    .IgnoreNullValues(true);

        //TypeAdapterConfig<PatchRegionCommand, Region>
        //    .NewConfig()
        //    .IgnoreNullValues(true);

        //TypeAdapterConfig<DeleteRegionRequest, DeleteRegionCommand>.NewConfig()
        //    .Map(dest => dest.RegionId, src => src.RegionID);

        // Map from RegionId to nullable Guid
        TypeAdapterConfig<RegionId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);


        // Map from nullable Guid to RegionId
        TypeAdapterConfig<Guid?, RegionId>
            .NewConfig()
            .MapWith(src => src.HasValue ? RegionId.Of(src.Value) : null);
    }
}
