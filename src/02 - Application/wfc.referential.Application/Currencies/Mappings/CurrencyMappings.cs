using Mapster;
using wfc.referential.Application.Currencies.Commands.CreateCurrency;
using wfc.referential.Application.Currencies.Commands.DeleteCurrency;
using wfc.referential.Application.Currencies.Commands.PatchCurrency;
using wfc.referential.Application.Currencies.Commands.UpdateCurrency;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Currencies.Queries;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Application.Currencies.Mappings;

public class CurrencyMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CurrencyResponse, Currency>.NewConfig()
            .ConstructUsing(src => Currency.Create(
                new CurrencyId(src.CurrencyId),
                src.Code,
                src.CodeAR,
                src.CodeEN,
                src.Name,
                src.CodeIso)
            );

        TypeAdapterConfig<Currency, CurrencyResponse>
            .NewConfig()
            .Map(dest => dest.CurrencyId, src => src.Id.Value)
            .Map(dest => dest.CountriesCount, src => src.Countries.Count)
            .Map(dest => dest.CodeAR, src => src.CodeAR)
            .Map(dest => dest.CodeEN, src => src.CodeEN)
            .Map(dest => dest.CodeIso, src => src.CodeIso)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<CreateCurrencyRequest, CreateCurrencyCommand>
            .NewConfig()
            .ConstructUsing(src => new CreateCurrencyCommand(
                src.Code,
                src.Name,
                src.CodeAR,
                src.CodeEN,
                src.CodeIso
            ));

        TypeAdapterConfig<DeleteCurrencyRequest, DeleteCurrencyCommand>
            .NewConfig()
            .ConstructUsing(src => new DeleteCurrencyCommand(src.CurrencyId));

        TypeAdapterConfig<UpdateCurrencyRequest, UpdateCurrencyCommand>
            .NewConfig()
            .ConstructUsing(src => new UpdateCurrencyCommand(
                src.CurrencyId,
                src.Code,
                src.Name,
                src.IsEnabled,
                src.CodeAR,
                src.CodeEN,
                src.CodeIso
            ));

        TypeAdapterConfig<GetAllCurrenciesRequest, GetAllCurrenciesQuery>
            .NewConfig()
            .ConstructUsing(src => new GetAllCurrenciesQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.Code,
                src.CodeAR,
                src.CodeEN,
                src.Name,
                src.CodeIso,
                src.IsEnabled
            ));

        TypeAdapterConfig<PatchCurrencyRequest, PatchCurrencyCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .MapToConstructor(true)
            .ConstructUsing(src => new PatchCurrencyCommand(
                src.CurrencyId,
                src.Code,
                src.Name,
                src.IsEnabled,
                src.CodeAR,
                src.CodeEN,
                src.CodeIso
            ));

        TypeAdapterConfig<PatchCurrencyCommand, Currency>
            .NewConfig()
            .IgnoreNullValues(true);
    }
}