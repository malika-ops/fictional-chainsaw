using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.UpdateCurrency;

public record UpdateCurrencyCommand : ICommand<Result<Guid>>
{
    public Guid CurrencyId { get; init; }
    public string Code { get; init; }
    public string Name { get; init; }
    public string CodeAR { get; init; }
    public string CodeEN { get; init; }
    public int CodeIso { get; init; } 
    public bool IsEnabled { get; init;}

    public UpdateCurrencyCommand(
        Guid currencyId,
        string code,
        string name,
        bool isEnabled,
        string codeAR,
        string codeEN,
        int codeiso)
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