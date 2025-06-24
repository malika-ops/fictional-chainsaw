using Mapster;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;

namespace wfc.referential.Application.PartnerCountries.Mappings;

public class PartnerCountryMappings
{
    public static void Register()
    {
        TypeAdapterConfig<PartnerCountryId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);


    }
}
