using Mapster;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Countries.Mappings;

public class CountryMappings
{
    public static void Register()
    {

        TypeAdapterConfig<CountryId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);
        TypeAdapterConfig<CountryId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);
    }
}
