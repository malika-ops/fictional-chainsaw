using Mapster;
using wfc.referential.Domain.AgencyTierAggregate;

namespace wfc.referential.Application.AgencyTiers.Mappings;

public class AgencyTierMappings
{
    public static void Register()
    {
        TypeAdapterConfig<AgencyTierId, Guid>
          .NewConfig()
          .MapWith(src => src.Value);
    }
}
