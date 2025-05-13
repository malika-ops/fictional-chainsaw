using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.BankAggregate.Exceptions;

namespace wfc.referential.Application.Banks.Commands.DeleteBank;

public class DeleteBankCommandHandler : ICommandHandler<DeleteBankCommand, bool>
{
    private readonly IBankRepository _bankRepository;

    public DeleteBankCommandHandler(IBankRepository bankRepository)
    {
        _bankRepository = bankRepository;
    }

    public async Task<bool> Handle(DeleteBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await _bankRepository.GetByIdAsync(BankId.Of(request.BankId), cancellationToken);

        if (bank == null)
            throw new InvalidBankDeletingException("Bank not found");

        // Check if bank has linked accounts
        var hasLinkedAccounts = await _bankRepository.HasLinkedAccountsAsync(bank.Id, cancellationToken);
        if (hasLinkedAccounts)
            throw new BankLinkedToAccountsException(request.BankId);

        // Disable the bank instead of deleting it
        bank.Disable();

        await _bankRepository.UpdateBankAsync(bank, cancellationToken);

        return true;
    }
}