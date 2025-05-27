using Mapster;
using wfc.referential.Application.Sectors.Commands.DeleteSector;
using wfc.referential.Application.Sectors.Dtos;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Sectors.Mappings;

public class SectorMappings
{
    public static void Register()
    {
        TypeAdapterConfig<SectorId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<CityId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<Sector, SectorResponse>
            .NewConfig()
            .Map(d => d.SectorId, s => s.Id.Value)
            .Map(d => d.CityId, s => s.CityId.Value)
            .Map(d => d.CityName, s => s.City != null ? s.City.Name : null);

        // Only explicit mappings needed where property names/types differ
        TypeAdapterConfig<DeleteSectorRequest, DeleteSectorCommand>
            .NewConfig()
            .MapWith(src => new DeleteSectorCommand(src.SectorId));
    }
}