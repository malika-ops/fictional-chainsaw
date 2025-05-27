using Mapster;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Tiers.Mappings;

public class TierMappings
{
    public static void Register()
    {
        TypeAdapterConfig<TierId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);
    }
}
