using Mapster;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Mappings;

public class TaxRuleDetailMappings
{
    public static void Register()
    {
        TypeAdapterConfig<TaxRuleDetail, GetFiltredTaxRuleDetailsResponse>
            .NewConfig()
            .Map(dest => dest.TaxRuleDetailsId, src => src.Id.Value);

        TypeAdapterConfig<CorridorId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<TaxId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<ServiceId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);
    }
}
