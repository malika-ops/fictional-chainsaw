using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.BankAggregate.Exceptions;

namespace wfc.referential.Application.Banks.Commands.UpdateBank;

public class UpdateBankCommandHandler : ICommandHandler<UpdateBankCommand, Guid>
{
    private readonly IBankRepository _bankRepository;

    public UpdateBankCommandHandler(IBankRepository bankRepository)
    {
        _bankRepository = bankRepository;
    }

    public async Task<Guid> Handle(UpdateBankCommand request, CancellationToken cancellationToken)
    {
        // Check if bank exists
        var bank = await _bankRepository.GetByIdAsync(new BankId(request.BankId), cancellationToken);
        if (bank is null)
            throw new BusinessException($"Bank with ID {request.BankId} not found");

        // Check if code is unique (if changed)
        var existingWithCode = await _bankRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingWithCode is not null && existingWithCode.Id.Value != request.BankId)
            throw new BankCodeAlreadyExistException(request.Code);

        // Update the bank
        bank.Update(request.Code, request.Name, request.Abbreviation);

        // Handle enabled status changes separately
        if (request.IsEnabled && !bank.IsEnabled)
        {
            bank.Activate();
        }
        else if (!request.IsEnabled && bank.IsEnabled)
        {
            bank.Disable();
        }

        await _bankRepository.UpdateBankAsync(bank, cancellationToken);

        return bank.Id.Value;
    }
}