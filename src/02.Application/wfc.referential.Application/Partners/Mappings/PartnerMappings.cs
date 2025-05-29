using Mapster;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Partners.Mappings;

public class PartnerMappings
{
    public static void Register()
    {
        TypeAdapterConfig<Partner, GetPartnersResponse>
            .NewConfig()
            .Map(dest => dest.PartnerId, src => src.Id.Value);

    }
}