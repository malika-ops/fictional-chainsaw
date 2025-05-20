using Mapster;
using wfc.referential.Application.Partners.Commands.PatchPartner;

namespace wfc.referential.Application.Partners.Mappings;

public class PartnerMappings
{
    public static void Register()
    {
        // Partner mappings
        TypeAdapterConfig<Domain.PartnerAggregate.Partner, Partners.Dtos.PartnerResponse>
            .NewConfig()
            .Map(dest => dest.PartnerId, src => src.Id.Value)
            .Map(dest => dest.TaxIdentificationNumber, src => src.TaxIdentificationNumber)
            .Map(dest => dest.ICE, src => src.ICE)
            .Map(dest => dest.RASRate, src => src.RASRate)
            .Map(dest => dest.NetworkMode, src => src.NetworkMode.ToString())
            .Map(dest => dest.PaymentMode, src => src.PaymentMode.ToString())
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.IdParent, src => src.IdParent)
            .Map(dest => dest.SupportAccountType, src => src.SupportAccountType.ToString())
            .Map(dest => dest.CommissionAccountId, src => src.CommissionAccountId)
            .Map(dest => dest.CommissionAccountName, src => src.CommissionAccount != null ? src.CommissionAccount.AccountNumber : "")
            .Map(dest => dest.ActivityAccountId, src => src.ActivityAccountId)
            .Map(dest => dest.ActivityAccountName, src => src.ActivityAccount != null ? src.ActivityAccount.AccountNumber : "")
            .Map(dest => dest.SupportAccountId, src => src.SupportAccountId)
            .Map(dest => dest.SupportAccountName, src => src.SupportAccount != null ? src.SupportAccount.Code : "")
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<Partners.Dtos.CreatePartnerRequest, Partners.Commands.CreatePartner.CreatePartnerCommand>
            .NewConfig()
            .ConstructUsing(src => new Partners.Commands.CreatePartner.CreatePartnerCommand(
                src.Code,
                src.Label,
                Enum.Parse<Domain.PartnerAggregate.NetworkMode>(src.NetworkMode),
                src.Type,
                Enum.Parse<Domain.PartnerAggregate.PaymentMode>(src.PaymentMode),
                Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType),
                src.TaxIdentificationNumber,
                src.TaxRegime,
                src.AuxiliaryAccount,
                src.ICE,
                src.RASRate,
                src.Logo,
                src.IdParent,
                src.CommissionAccountId,
                src.ActivityAccountId,
                src.SupportAccountId
            ));

        TypeAdapterConfig<Partners.Dtos.UpdatePartnerRequest, Partners.Commands.UpdatePartner.UpdatePartnerCommand>
            .NewConfig()
            .ConstructUsing(src => new Partners.Commands.UpdatePartner.UpdatePartnerCommand(
                src.PartnerId,
                src.Code,
                src.Label,
                Enum.Parse<Domain.PartnerAggregate.NetworkMode>(src.NetworkMode),
                src.Type,
                Enum.Parse<Domain.PartnerAggregate.PaymentMode>(src.PaymentMode),
                Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType),
                src.TaxIdentificationNumber,
                src.TaxRegime,
                src.AuxiliaryAccount,
                src.ICE,
                src.RASRate,
                src.IsEnabled,
                src.Logo,
                src.IdParent,
                src.CommissionAccountId,
                src.ActivityAccountId,
                src.SupportAccountId
            ));

        TypeAdapterConfig<Partners.Dtos.DeletePartnerRequest, Partners.Commands.DeletePartner.DeletePartnerCommand>
            .NewConfig()
            .ConstructUsing(src => new Partners.Commands.DeletePartner.DeletePartnerCommand(
                src.PartnerId
            ));

        TypeAdapterConfig<Partners.Dtos.PatchPartnerRequest, Partners.Commands.PatchPartner.PatchPartnerCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .MapToConstructor(true)
            .ConstructUsing(src => new Partners.Commands.PatchPartner.PatchPartnerCommand(
                src.PartnerId,
                src.Code,
                src.Label,
                src.NetworkMode != null ? (Domain.PartnerAggregate.NetworkMode?)Enum.Parse<Domain.PartnerAggregate.NetworkMode>(src.NetworkMode) : null,
                src.PaymentMode != null ? (Domain.PartnerAggregate.PaymentMode?)Enum.Parse<Domain.PartnerAggregate.PaymentMode>(src.PaymentMode) : null,
                src.Type,
                src.SupportAccountType != null ? (Domain.SupportAccountAggregate.SupportAccountType?)Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType) : null,
                src.TaxIdentificationNumber,
                src.TaxRegime,
                src.AuxiliaryAccount,
                src.ICE,
                src.RASRate,
                src.IsEnabled,
                src.Logo,
                src.IdParent,
                src.CommissionAccountId,
                src.ActivityAccountId,
                src.SupportAccountId
            ));

        TypeAdapterConfig<Partners.Commands.PatchPartner.PatchPartnerCommand, Domain.PartnerAggregate.Partner>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<Partners.Dtos.GetAllPartnersRequest, Partners.Queries.GetAllPartners.GetAllPartnersQuery>
            .NewConfig()
            .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
            .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
            .ConstructUsing(src => new Partners.Queries.GetAllPartners.GetAllPartnersQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.Code,
                src.Label,
                src.Type,
                src.NetworkMode,
                src.PaymentMode,
                src.IdParent,
                src.SupportAccountType,
                src.TaxIdentificationNumber,
                src.ICE,
                src.CommissionAccountId,
                src.ActivityAccountId,
                src.SupportAccountId,
                src.IsEnabled
            ));
    }
}