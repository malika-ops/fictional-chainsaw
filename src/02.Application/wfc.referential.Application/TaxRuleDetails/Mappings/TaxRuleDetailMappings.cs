using Mapster;
using wfc.referential.Application.TaxRuleDetails.Commands.CreateTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Commands.PatchTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Commands.UpdateTaxRuleDetail;
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
        TypeAdapterConfig<CreateTaxRuleDetailRequest, CreateTaxRuleDetailCommand>
            .NewConfig().IgnoreNullValues(true);

        
        TypeAdapterConfig<CreateTaxRuleDetailCommand, TaxRuleDetail>
            .NewConfig()
            .Map(dest => dest.Id, src => src.TaxRuleDetailsId);

        
        TypeAdapterConfig<TaxRuleDetail, CreateTaxRuleDetailResponse>
            .NewConfig()
            .Map(dest => dest.TaxRuleDetailsId, src => src.Id.Value);


        TypeAdapterConfig<UpdateTaxRuleDetailRequest, UpdateTaxRuleDetailCommand>
            .NewConfig().IgnoreNullValues(true);


        TypeAdapterConfig<TaxRuleDetail, GetAllTaxRuleDetailsResponse>
            .NewConfig()
            .Map(dest => dest.TaxRuleDetailsId, src => src.Id.Value);

        TypeAdapterConfig<DeleteTaxRuleDetailRequest, DeleteTaxRuleDetailCommand>
            .NewConfig()
            .Map(dest => dest.TaxRuleDetailsId, src => src.TaxRuleDetailId);

        TypeAdapterConfig<PatchTaxRuleDetailCommand, TaxRuleDetail>
            .NewConfig()
            .IgnoreNullValues(true)
            .Map(dest => dest.Id, src => src.CorridorId);

        TypeAdapterConfig<Guid, CorridorId>
            .NewConfig()
            .MapWith(src => CorridorId.Of(src));

        TypeAdapterConfig<Guid, TaxId>
            .NewConfig()
            .MapWith(src => TaxId.Of(src));

        TypeAdapterConfig<Guid, ServiceId>
            .NewConfig()
            .MapWith(src => ServiceId.Of(src));

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
