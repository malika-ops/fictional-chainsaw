using Mapster;
using wfc.referential.Application.RegionManagement.Dtos;
using wfc.referential.Application.RegionManagement.Queries.GetFiltredRegions;
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


        TypeAdapterConfig<Region, GetRegionsResponse>
            .NewConfig()
            .Map(dest => dest.CountryId, src => src.CountryId.Value)
            .Map(dest => dest.RegionId, src => src.Id.Value);
        // Map from RegionId to nullable Guid
        //TypeAdapterConfig<RegionId, Guid?>
        //    .NewConfig()
        //    .MapWith(src => src == null ? (Guid?)null : src.Value);


        //// Map from nullable Guid to RegionId
        //TypeAdapterConfig<Guid?, RegionId>
        //    .NewConfig()
        //    .MapWith(src => src.HasValue ? RegionId.Of(src.Value) : null);
    }
}
