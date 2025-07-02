using Mapster;
using wfc.referential.Application.ContractDetails.Dtos;
using wfc.referential.Domain.ContractDetailsAggregate;

namespace wfc.referential.Application.ContractDetails.Mappings;

public class ContractDetailsMappings
{
    public static void Register()
    {
        TypeAdapterConfig<Domain.ContractDetailsAggregate.ContractDetails, GetContractDetailsResponse>
            .NewConfig()
            .Map(dest => dest.ContractDetailsId, src => src.Id.Value)
            .Map(dest => dest.ContractId, src => src.ContractId.Value)
            .Map(dest => dest.PricingId, src => src.PricingId.Value);
    }
}