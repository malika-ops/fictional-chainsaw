using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.Banks.Commands.UpdateBank;

public class UpdateBankCommand : ICommand<Guid>
{
    public Guid BankId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Abbreviation { get; set; }
    public bool IsEnabled { get; set; }

    public UpdateBankCommand(Guid bankId, string code, string name, string abbreviation, bool isEnabled)
    {
        BankId = bankId;
        Code = code;
        Name = name;
        Abbreviation = abbreviation;
        IsEnabled = isEnabled;
    }
}