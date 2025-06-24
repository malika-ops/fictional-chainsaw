using Mapster;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.SupportAccounts.Dtos;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Corridors.Mappings;

public class CorridorMappings
{
    public static void Register()
    {

        TypeAdapterConfig<CorridorId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

    }
}
