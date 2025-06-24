using Mapster;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.MonetaryZones.Mappings;

public class MonetaryZoneMappings
{
    public static void Register()
    {
       
        TypeAdapterConfig<MonetaryZoneId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);
    }
}
