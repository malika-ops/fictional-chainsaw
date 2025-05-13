using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.BankAggregate.Exceptions;

namespace wfc.referential.Application.Banks.Commands.PatchBank;

public class PatchBankCommandHandler : ICommandHandler<PatchBankCommand, Guid>
{
    private readonly IBankRepository _bankRepository;

    public PatchBankCommandHandler(IBankRepository bankRepository)
    {
        _bankRepository = bankRepository;
    }

    public async Task<Guid> Handle(PatchBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await _bankRepository.GetByIdAsync(new BankId(request.BankId), cancellationToken);
        if (bank == null)
            throw new BusinessException("Bank not found");

        // Check if code is unique if it's being updated
        if (request.Code != null && request.Code != bank.Code)
        {
            var existingWithCode = await _bankRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingWithCode != null && existingWithCode.Id.Value != request.BankId)
                throw new BankCodeAlreadyExistException(request.Code);
        }

        // Collect updates for domain entities
        var updatedCode = request.Code ?? bank.Code;
        var updatedName = request.Name ?? bank.Name;
        var updatedAbbreviation = request.Abbreviation ?? bank.Abbreviation;

        // Update via domain methods
        bank.Patch(updatedCode, updatedName, updatedAbbreviation);

        // Handle IsEnabled status changes separately through the proper domain methods
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value && !bank.IsEnabled)
            {
                bank.Activate();
            }
            else if (!request.IsEnabled.Value && bank.IsEnabled)
            {
                bank.Disable();
            }
        }

        await _bankRepository.UpdateBankAsync(bank, cancellationToken);

        return bank.Id.Value;
    }
}