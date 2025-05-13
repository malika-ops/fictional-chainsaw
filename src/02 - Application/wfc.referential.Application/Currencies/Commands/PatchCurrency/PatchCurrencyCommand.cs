using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.PatchCurrency;

public class PatchCurrencyCommand : ICommand<Result<Guid>>
{
    public Guid CurrencyId { get; }
    public string? Code { get; }
    public string? Name { get; }
    public string? CodeAR { get; }
    public string? CodeEN { get; }
    public int? CodeIso { get; }
    public bool? IsEnabled { get; }

    public PatchCurrencyCommand(
        Guid currencyId,
        string? code = null,
        string? name = null,
        bool? isEnabled = null,
        string? codeAR = null,
        string? codeEN = null,
        int? codeiso = null)
    {
        CurrencyId = currencyId;
        Code = code;
        Name = name;
        IsEnabled = isEnabled;
        CodeAR = codeAR;
        CodeEN = codeEN;
        CodeIso = codeiso;
    }
}