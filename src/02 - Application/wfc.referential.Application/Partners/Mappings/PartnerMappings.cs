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
            .Map(dest => dest.ICE, src => src.ICE)
            .Map(dest => dest.NetworkMode, src => src.NetworkMode.ToString())
            .Map(dest => dest.PaymentMode, src => src.PaymentMode.ToString())
            .Map(dest => dest.SupportAccountType, src => src.SupportAccountType.ToString())
            .Map(dest => dest.SectorId, src => src.Sector.Id.Value)
            .Map(dest => dest.SectorName, src => src.Sector.Name)
            .Map(dest => dest.CityId, src => src.City.Id.Value)
            .Map(dest => dest.CityName, src => src.City.Name)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<Partners.Dtos.CreatePartnerRequest, Partners.Commands.CreatePartner.CreatePartnerCommand>
            .NewConfig()
            .ConstructUsing(src => new Partners.Commands.CreatePartner.CreatePartnerCommand(
                src.Code,
                src.Label,
                Enum.Parse<Domain.PartnerAggregate.NetworkMode>(src.NetworkMode),
                Enum.Parse<Domain.PartnerAggregate.PaymentMode>(src.PaymentMode),
                src.IdPartner,
                Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType),
                src.IdentificationNumber,
                src.TaxRegime,
                src.AuxiliaryAccount,
                src.ICE,
                src.Logo,
                src.SectorId,
                src.CityId
            ));

        TypeAdapterConfig<Partners.Dtos.UpdatePartnerRequest, Partners.Commands.UpdatePartner.UpdatePartnerCommand>
            .NewConfig()
            .ConstructUsing(src => new Partners.Commands.UpdatePartner.UpdatePartnerCommand(
                src.PartnerId,
                src.Code,
                src.Label,
                Enum.Parse<Domain.PartnerAggregate.NetworkMode>(src.NetworkMode),
                Enum.Parse<Domain.PartnerAggregate.PaymentMode>(src.PaymentMode),
                src.IdPartner,
                Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType),
                src.IdentificationNumber,
                src.TaxRegime,
                src.AuxiliaryAccount,
                src.ICE,
                src.IsEnabled,
                src.Logo,
                src.SectorId,
                src.CityId
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
                src.IdPartner,
                src.SupportAccountType != null ? (Domain.SupportAccountAggregate.SupportAccountType?)Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType) : null,
                src.IdentificationNumber,
                src.TaxRegime,
                src.AuxiliaryAccount,
                src.ICE,
                src.IsEnabled,
                src.Logo,
                src.SectorId,
                src.CityId
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
                src.NetworkMode,
                src.PaymentMode,
                src.IdPartner,
                src.SupportAccountType,
                src.IdentificationNumber,
                src.ICE,
                src.SectorId,
                src.CityId,
                src.IsEnabled
            ));
    }
}