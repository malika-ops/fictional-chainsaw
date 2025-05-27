using Mapster;
using wfc.referential.Application.SupportAccounts.Dtos;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.SupportAccounts.Mappings;

public class SupportAccountMappings
{
    public static void Register()
    {
        TypeAdapterConfig<SupportAccountId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<PartnerId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<ParamTypeId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<SupportAccount, GetSupportAccountsResponse>
            .NewConfig()
            .Map(d => d.SupportAccountId, s => s.Id.Value)
            .Map(d => d.PartnerId, s => s.PartnerId != null ? s.PartnerId.Value : (Guid?)null)
            .Map(d => d.SupportAccountTypeId, s => s.SupportAccountTypeId != null ? s.SupportAccountTypeId.Value : (Guid?)null);
    }
}