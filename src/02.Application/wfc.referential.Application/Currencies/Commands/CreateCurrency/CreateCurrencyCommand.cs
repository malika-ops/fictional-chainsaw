using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.CreateCurrency;

public class CreateCurrencyCommand : ICommand<Result<Guid>>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string CodeAR { get; set; }
    public string CodeEN { get; set; }
    public int CodeIso { get; set; }

    public CreateCurrencyCommand(
        string code,
        string name,
        string codeAR = null,
        string codeEN = null,
        int codeIso = 0)
    {
        Code = code;
        Name = name;
        CodeAR = codeAR;
        CodeEN = codeEN;
        CodeIso = codeIso;
    }
}