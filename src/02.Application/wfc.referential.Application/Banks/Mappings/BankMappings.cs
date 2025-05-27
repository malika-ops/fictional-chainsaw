using Mapster;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Domain.BankAggregate;

namespace wfc.referential.Application.Banks.Mappings;

public class BankMappings
{
    public static void Register()
    {
        TypeAdapterConfig<BankId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<Bank, GetBanksResponse>
            .NewConfig()
            .Map(d => d.BankId, s => s.Id.Value);
    }
}