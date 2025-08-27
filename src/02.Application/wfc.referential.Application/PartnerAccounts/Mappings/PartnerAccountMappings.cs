using Mapster;
using wfc.referential.Application.PartnerAccounts.Dtos;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Mappings;

public class PartnerAccountMappings
{
    public static void Register()
    {
        TypeAdapterConfig<PartnerAccountId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<PartnerAccount, PartnerAccountResponse>
            .NewConfig()
            .Map(d => d.PartnerAccountId, s => s.Id.Value)
            .Map(d => d.BankId, s => s.BankId.Value)
            .Map(d => d.BankName, s => s.Bank.Name)
            .Map(d => d.BankCode, s => s.Bank.Code);
    }
}