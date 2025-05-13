using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;

public class UpdatePartnerAccountCommandHandler : ICommandHandler<UpdatePartnerAccountCommand, Guid>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly IBankRepository _bankRepository;

    public UpdatePartnerAccountCommandHandler(
        IPartnerAccountRepository partnerAccountRepository,
        IBankRepository bankRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
        _bankRepository = bankRepository;
    }

    public async Task<Guid> Handle(UpdatePartnerAccountCommand request, CancellationToken cancellationToken)
    {
        // Check if partner account exists
        var partnerAccount = await _partnerAccountRepository.GetByIdAsync(new PartnerAccountId(request.PartnerAccountId), cancellationToken);
        if (partnerAccount is null)
            throw new BusinessException($"Partner account with ID {request.PartnerAccountId} not found");

        // Check if account number is unique (if changed)
        if (request.AccountNumber != partnerAccount.AccountNumber)
        {
            var existingWithAccountNumber = await _partnerAccountRepository.GetByAccountNumberAsync(request.AccountNumber, cancellationToken);
            if (existingWithAccountNumber is not null && existingWithAccountNumber.Id.Value != request.PartnerAccountId)
                throw new PartnerAccountAlreadyExistException(request.AccountNumber);
        }

        // Check if RIB is unique (if changed)
        if (request.RIB != partnerAccount.RIB)
        {
            var existingWithRIB = await _partnerAccountRepository.GetByRIBAsync(request.RIB, cancellationToken);
            if (existingWithRIB is not null && existingWithRIB.Id.Value != request.PartnerAccountId)
                throw new BusinessException($"Partner account with RIB {request.RIB} already exists.");
        }

        // Get the bank
        var bank = await _bankRepository.GetByIdAsync(new BankId(request.BankId), cancellationToken);
        if (bank is null)
            throw new BusinessException($"Bank with ID {request.BankId} not found");

        // Update the partner account
        partnerAccount.Update(
            request.AccountNumber,
            request.RIB,
            request.Domiciliation,
            request.BusinessName,
            request.ShortName,
            request.AccountBalance,
            bank,
            request.AccountType
        );

        // Handle enabled status changes through the proper domain methods
        if (request.IsEnabled && !partnerAccount.IsEnabled)
        {
            partnerAccount.Activate();
        }
        else if (!request.IsEnabled && partnerAccount.IsEnabled)
        {
            partnerAccount.Disable();
        }

        await _partnerAccountRepository.UpdatePartnerAccountAsync(partnerAccount, cancellationToken);

        return partnerAccount.Id.Value;
    }
}