using Mapster;

namespace wfc.referential.Application.SupportAccounts.Mappings;

public class SupportAccountMappings
{
    public static void Register()
    {
        // SupportAccount mappings
        TypeAdapterConfig<Domain.SupportAccountAggregate.SupportAccount, SupportAccounts.Dtos.SupportAccountResponse>
            .NewConfig()
            .Map(dest => dest.SupportAccountId, src => src.Id.Value)
            .Map(dest => dest.PartnerId, src => src.Partner.Id.Value)
            .Map(dest => dest.PartnerCode, src => src.Partner.Code)
            .Map(dest => dest.PartnerLabel, src => src.Partner.Label)
            .Map(dest => dest.SupportAccountType, src => src.SupportAccountType.ToString())
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<SupportAccounts.Dtos.CreateSupportAccountRequest, SupportAccounts.Commands.CreateSupportAccount.CreateSupportAccountCommand>
            .NewConfig()
            .ConstructUsing(src => new SupportAccounts.Commands.CreateSupportAccount.CreateSupportAccountCommand(
                src.Code,
                src.Name,
                src.Threshold,
                src.Limit,
                src.AccountBalance,
                src.AccountingNumber,
                src.PartnerId,
                Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType)
            ));

        TypeAdapterConfig<SupportAccounts.Dtos.UpdateSupportAccountRequest, SupportAccounts.Commands.UpdateSupportAccount.UpdateSupportAccountCommand>
            .NewConfig()
            .ConstructUsing(src => new SupportAccounts.Commands.UpdateSupportAccount.UpdateSupportAccountCommand(
                src.SupportAccountId,
                src.Code,
                src.Name,
                src.Threshold,
                src.Limit,
                src.AccountBalance,
                src.AccountingNumber,
                src.PartnerId,
                Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType),
                src.IsEnabled
            ));

        TypeAdapterConfig<SupportAccounts.Dtos.DeleteSupportAccountRequest, SupportAccounts.Commands.DeleteSupportAccount.DeleteSupportAccountCommand>
            .NewConfig()
            .ConstructUsing(src => new SupportAccounts.Commands.DeleteSupportAccount.DeleteSupportAccountCommand(
                src.SupportAccountId
            ));

        TypeAdapterConfig<SupportAccounts.Dtos.PatchSupportAccountRequest, SupportAccounts.Commands.PatchSupportAccount.PatchSupportAccountCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .MapToConstructor(true)
            .ConstructUsing(src => new SupportAccounts.Commands.PatchSupportAccount.PatchSupportAccountCommand(
                src.SupportAccountId,
                src.Code,
                src.Name,
                src.Threshold,
                src.Limit,
                src.AccountBalance,
                src.AccountingNumber,
                src.PartnerId,
                src.SupportAccountType != null ? (Domain.SupportAccountAggregate.SupportAccountType?)Enum.Parse<Domain.SupportAccountAggregate.SupportAccountType>(src.SupportAccountType) : null,
                src.IsEnabled
            ));

        TypeAdapterConfig<SupportAccounts.Commands.PatchSupportAccount.PatchSupportAccountCommand, Domain.SupportAccountAggregate.SupportAccount>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<SupportAccounts.Dtos.GetAllSupportAccountsRequest, SupportAccounts.Queries.GetAllSupportAccounts.GetAllSupportAccountsQuery>
            .NewConfig()
            .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
            .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
            .ConstructUsing(src => new SupportAccounts.Queries.GetAllSupportAccounts.GetAllSupportAccountsQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.Code,
                src.Name,
                src.MinThreshold,
                src.MaxThreshold,
                src.MinLimit,
                src.MaxLimit,
                src.MinAccountBalance,
                src.MaxAccountBalance,
                src.AccountingNumber,
                src.PartnerId,
                src.SupportAccountType,
                src.IsEnabled
            ));

        TypeAdapterConfig<SupportAccounts.Dtos.UpdateSupportAccountBalanceRequest, SupportAccounts.Commands.UpdateBalance.UpdateSupportAccountBalanceCommand>
            .NewConfig()
            .ConstructUsing(src => new SupportAccounts.Commands.UpdateBalance.UpdateSupportAccountBalanceCommand(
                src.SupportAccountId,
                src.NewBalance
            ));
    }
}