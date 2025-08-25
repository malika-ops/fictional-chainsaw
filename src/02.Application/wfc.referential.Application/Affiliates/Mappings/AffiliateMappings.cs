using Mapster;
using wfc.referential.Application.Affiliates.Dtos;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Affiliates.Mappings;

public class AffiliateMappings
{
    public static void Register()
    {
        TypeAdapterConfig<Affiliate, GetAffiliatesResponse>
             .NewConfig()
             .Map(dest => dest.AffiliateId, src => src.Id.Value)
             .Map(dest => dest.CountryId, src => src.CountryId.Value);
        TypeAdapterConfig<AffiliateId, Guid?>
              .NewConfig()
              .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<AffiliateId, Guid>
       .NewConfig()
       .MapWith(src => src.Value);
    }
}