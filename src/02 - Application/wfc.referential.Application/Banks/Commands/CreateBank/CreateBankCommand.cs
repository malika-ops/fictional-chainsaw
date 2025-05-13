using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Banks.Commands.CreateBank;

public class CreateBankCommand : ICommand<Result<Guid>>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Abbreviation { get; set; }

    public CreateBankCommand(string code, string name, string abbreviation)
    {
        Code = code;
        Name = name;
        Abbreviation = abbreviation;
    }
}