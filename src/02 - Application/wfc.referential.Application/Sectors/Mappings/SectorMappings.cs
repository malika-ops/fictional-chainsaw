using Mapster;

namespace wfc.referential.Application.Sectors.Mappings;

public class SectorMappings
{
    public static void Register()
    {
        // Sector mappings
        TypeAdapterConfig<Domain.SectorAggregate.Sector, Sectors.Dtos.SectorResponse>
            .NewConfig()
            .Map(dest => dest.SectorId, src => src.Id.Value)
            .Map(dest => dest.CityId, src => src.City.Id.Value)
            .Map(dest => dest.CityName, src => src.City.Name)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);


        TypeAdapterConfig<Sectors.Dtos.CreateSectorRequest, Sectors.Commands.CreateSector.CreateSectorCommand>
            .NewConfig()
            .ConstructUsing(src => new Sectors.Commands.CreateSector.CreateSectorCommand(
                src.Code,
                src.Name,
                src.CityId
            ));

        TypeAdapterConfig<Sectors.Dtos.UpdateSectorRequest, Sectors.Commands.UpdateSector.UpdateSectorCommand>
            .NewConfig()
            .ConstructUsing(src => new Sectors.Commands.UpdateSector.UpdateSectorCommand(
                src.SectorId,
                src.Code,
                src.Name,
                src.CityId,
                src.IsEnabled
            ));

        TypeAdapterConfig<Sectors.Dtos.DeleteSectorRequest, Sectors.Commands.DeleteSector.DeleteSectorCommand>
            .NewConfig()
            .ConstructUsing(src => new Sectors.Commands.DeleteSector.DeleteSectorCommand(
                src.SectorId
            ));

        TypeAdapterConfig<Sectors.Dtos.PatchSectorRequest, Sectors.Commands.PatchSector.PatchSectorCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .MapToConstructor(true)
            .ConstructUsing(src => new Sectors.Commands.PatchSector.PatchSectorCommand(
                src.SectorId,
                src.Code,
                src.Name,
                src.CityId,
                src.IsEnabled
                ));

        TypeAdapterConfig<Sectors.Commands.PatchSector.PatchSectorCommand, Domain.SectorAggregate.Sector>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<Sectors.Dtos.GetAllSectorsRequest, Sectors.Queries.GetAllSectors.GetAllSectorsQuery>
            .NewConfig()
            .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
            .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
            .ConstructUsing(src => new Sectors.Queries.GetAllSectors.GetAllSectorsQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.Code,
                src.Name,
                src.CityId,
                src.IsEnabled
            ));
    }
}