using Mapster;
using wfc.referential.Application.Contracts.Dtos;
using wfc.referential.Domain.ContractAggregate;

namespace wfc.referential.Application.Contracts.Mappings;

public class ContractMappings
{
    public static void Register()
    {
        TypeAdapterConfig<Contract, GetContractsResponse>
            .NewConfig()
            .Map(dest => dest.ContractId, src => src.Id.Value)
            .Map(dest => dest.PartnerId, src => src.PartnerId.Value);
    }
}