using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.Banks.Commands.DeleteBank;

public class DeleteBankCommand : ICommand<bool>
{
    public Guid BankId { get; set; }

    public DeleteBankCommand(Guid bankId)
    {
        BankId = bankId;
    }
}