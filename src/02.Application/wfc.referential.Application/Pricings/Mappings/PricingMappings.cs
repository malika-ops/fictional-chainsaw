using Mapster;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Application.Pricings.Mappings;

public class PricingMappings
{
    public static void Register()
    {
        TypeAdapterConfig<PricingId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

    }
}
