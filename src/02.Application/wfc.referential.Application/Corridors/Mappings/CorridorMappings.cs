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



    }
}
