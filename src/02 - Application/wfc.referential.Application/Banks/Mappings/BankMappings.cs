using Mapster;

namespace wfc.referential.Application.Banks.Mappings;

public class BankMappings
{
    public static void Register()
    {
        // Bank mappings
        TypeAdapterConfig<Domain.BankAggregate.Bank, Banks.Dtos.BankResponse>
            .NewConfig()
            .Map(dest => dest.BankId, src => src.Id.Value)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);


        TypeAdapterConfig<Banks.Dtos.CreateBankRequest, Banks.Commands.CreateBank.CreateBankCommand>
            .NewConfig()
            .ConstructUsing(src => new Banks.Commands.CreateBank.CreateBankCommand(
                src.Code,
                src.Name,
                src.Abbreviation
            ));

        TypeAdapterConfig<Banks.Dtos.UpdateBankRequest, Banks.Commands.UpdateBank.UpdateBankCommand>
            .NewConfig()
            .ConstructUsing(src => new Banks.Commands.UpdateBank.UpdateBankCommand(
                src.BankId,
                src.Code,
                src.Name,
                src.Abbreviation,
                src.IsEnabled
            ));

        TypeAdapterConfig<Banks.Dtos.DeleteBankRequest, Banks.Commands.DeleteBank.DeleteBankCommand>
            .NewConfig()
            .ConstructUsing(src => new Banks.Commands.DeleteBank.DeleteBankCommand(
                src.BankId
            ));

        TypeAdapterConfig<Banks.Dtos.PatchBankRequest, Banks.Commands.PatchBank.PatchBankCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .MapToConstructor(true)
            .ConstructUsing(src => new Banks.Commands.PatchBank.PatchBankCommand(
                src.BankId,
                src.Code,
                src.Name,
                src.Abbreviation,
                src.IsEnabled
                ));

        TypeAdapterConfig<Banks.Commands.PatchBank.PatchBankCommand, Domain.BankAggregate.Bank>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<Banks.Dtos.GetAllBanksRequest, Banks.Queries.GetAllBanks.GetAllBanksQuery>
            .NewConfig()
            .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
            .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
            .ConstructUsing(src => new Banks.Queries.GetAllBanks.GetAllBanksQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.Code,
                src.Name,
                src.Abbreviation,
                src.IsEnabled
            ));
    }
}