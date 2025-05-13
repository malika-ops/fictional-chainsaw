using Mapster;

namespace wfc.referential.Application.PartnerAccounts.Mappings;

public class PartnerAccountMappings
{
    public static void Register()
    {
        // PartnerAccount mappings
        TypeAdapterConfig<Domain.PartnerAccountAggregate.PartnerAccount, PartnerAccounts.Dtos.PartnerAccountResponse>
            .NewConfig()
            .Map(dest => dest.PartnerAccountId, src => src.Id.Value)
            .Map(dest => dest.RIB, src => src.RIB)
            .Map(dest => dest.BankId, src => src.Bank.Id.Value)
            .Map(dest => dest.BankName, src => src.Bank.Name)
            .Map(dest => dest.BankCode, src => src.Bank.Code)
            .Map(dest => dest.AccountType, src => src.AccountType.ToString())
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<PartnerAccounts.Dtos.CreatePartnerAccountRequest, PartnerAccounts.Commands.CreatePartnerAccount.CreatePartnerAccountCommand>
            .NewConfig()
            .ConstructUsing(src => new PartnerAccounts.Commands.CreatePartnerAccount.CreatePartnerAccountCommand(
                src.AccountNumber,
                src.RIB,
                src.Domiciliation,
                src.BusinessName,
                src.ShortName,
                src.AccountBalance,
                src.BankId,
                Enum.Parse<Domain.PartnerAccountAggregate.AccountType>(src.AccountType)
            ));

        TypeAdapterConfig<PartnerAccounts.Dtos.UpdatePartnerAccountRequest, PartnerAccounts.Commands.UpdatePartnerAccount.UpdatePartnerAccountCommand>
            .NewConfig()
            .ConstructUsing(src => new PartnerAccounts.Commands.UpdatePartnerAccount.UpdatePartnerAccountCommand(
                src.PartnerAccountId,
                src.AccountNumber,
                src.RIB,
                src.Domiciliation,
                src.BusinessName,
                src.ShortName,
                src.AccountBalance,
                src.BankId,
                Enum.Parse<Domain.PartnerAccountAggregate.AccountType>(src.AccountType),
                src.IsEnabled
            ));

        TypeAdapterConfig<PartnerAccounts.Dtos.DeletePartnerAccountRequest, PartnerAccounts.Commands.DeletePartnerAccount.DeletePartnerAccountCommand>
            .NewConfig()
            .ConstructUsing(src => new PartnerAccounts.Commands.DeletePartnerAccount.DeletePartnerAccountCommand(
                src.PartnerAccountId
            ));

        TypeAdapterConfig<PartnerAccounts.Dtos.PatchPartnerAccountRequest, PartnerAccounts.Commands.PatchPartnerAccount.PatchPartnerAccountCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .MapToConstructor(true)
            .ConstructUsing(src => new PartnerAccounts.Commands.PatchPartnerAccount.PatchPartnerAccountCommand(
                src.PartnerAccountId,
                src.AccountNumber,
                src.RIB,
                src.Domiciliation,
                src.BusinessName,
                src.ShortName,
                src.AccountBalance,
                src.BankId,
                src.AccountType != null ? (Domain.PartnerAccountAggregate.AccountType?)Enum.Parse<Domain.PartnerAccountAggregate.AccountType>(src.AccountType) : null,
                src.IsEnabled
            ));

        TypeAdapterConfig<PartnerAccounts.Commands.PatchPartnerAccount.PatchPartnerAccountCommand, Domain.PartnerAccountAggregate.PartnerAccount>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<PartnerAccounts.Dtos.GetAllPartnerAccountsRequest, PartnerAccounts.Queries.GetAllPartnerAccounts.GetAllPartnerAccountsQuery>
            .NewConfig()
            .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
            .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
            .ConstructUsing(src => new PartnerAccounts.Queries.GetAllPartnerAccounts.GetAllPartnerAccountsQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.AccountNumber,
                src.RIB,
                src.BusinessName,
                src.ShortName,
                src.MinAccountBalance,
                src.MaxAccountBalance,
                src.BankId,
                src.AccountType,
                src.IsEnabled
            ));

        TypeAdapterConfig<PartnerAccounts.Dtos.UpdateBalanceRequest, PartnerAccounts.Commands.UpdateBalance.UpdateBalanceCommand>
            .NewConfig()
            .ConstructUsing(src => new PartnerAccounts.Commands.UpdateBalance.UpdateBalanceCommand(
                src.PartnerAccountId,
                src.NewBalance
            ));
    }
}