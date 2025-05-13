using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.UpdateCurrency;

public class UpdateCurrencyCommand : ICommand<Result<Guid>>
{
    public Guid CurrencyId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string CodeAR { get; set; }
    public string CodeEN { get; set; }
    public int CodeIso { get; set; }
    public bool IsEnabled { get; set; }

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