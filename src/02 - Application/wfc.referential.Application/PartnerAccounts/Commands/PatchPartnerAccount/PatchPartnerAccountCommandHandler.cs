using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

namespace wfc.referential.Application.PartnerAccounts.Commands.PatchPartnerAccount;

public class PatchPartnerAccountCommandHandler : ICommandHandler<PatchPartnerAccountCommand, Guid>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly IBankRepository _bankRepository;

    public PatchPartnerAccountCommandHandler(
        IPartnerAccountRepository partnerAccountRepository,
        IBankRepository bankRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
        _bankRepository = bankRepository;
    }

    public async Task<Guid> Handle(PatchPartnerAccountCommand request, CancellationToken cancellationToken)
    {
        var partnerAccount = await _partnerAccountRepository.GetByIdAsync(new PartnerAccountId(request.PartnerAccountId), cancellationToken);
        if (partnerAccount == null)
            throw new BusinessException("Partner account not found");

        // Check if account number is unique if it's being updated
        if (request.AccountNumber != null && request.AccountNumber != partnerAccount.AccountNumber)
        {
            var existingWithAccountNumber = await _partnerAccountRepository.GetByAccountNumberAsync(request.AccountNumber, cancellationToken);
            if (existingWithAccountNumber != null && existingWithAccountNumber.Id.Value != request.PartnerAccountId)
                throw new PartnerAccountAlreadyExistException(request.AccountNumber);
        }

        // Check if RIB is unique if it's being updated
        if (request.RIB != null && request.RIB != partnerAccount.RIB)
        {
            var existingWithRIB = await _partnerAccountRepository.GetByRIBAsync(request.RIB, cancellationToken);
            if (existingWithRIB != null && existingWithRIB.Id.Value != request.PartnerAccountId)
                throw new BusinessException($"Partner account with RIB {request.RIB} already exists.");
        }

        // Get updated bank if needed
        var bank = partnerAccount.Bank;
        if (request.BankId.HasValue && request.BankId.Value != partnerAccount.Bank.Id.Value)
        {
            var updatedBank = await _bankRepository.GetByIdAsync(new BankId(request.BankId.Value), cancellationToken);
            if (updatedBank == null)
                throw new BusinessException($"Bank with ID {request.BankId} not found");
            bank = updatedBank;
        }

        // Collect updates for domain entities
        var updatedAccountNumber = request.AccountNumber ?? partnerAccount.AccountNumber;
        var updatedRIB = request.RIB ?? partnerAccount.RIB;
        var updatedDomiciliation = request.Domiciliation ?? partnerAccount.Domiciliation;
        var updatedBusinessName = request.BusinessName ?? partnerAccount.BusinessName;
        var updatedShortName = request.ShortName ?? partnerAccount.ShortName;
        var updatedAccountBalance = request.AccountBalance ?? partnerAccount.AccountBalance;
        var updatedAccountType = request.AccountType ?? partnerAccount.AccountType;

        // Update via domain methods
        partnerAccount.Patch(
            updatedAccountNumber,
            updatedRIB,
            updatedDomiciliation,
            updatedBusinessName,
            updatedShortName,
            updatedAccountBalance,
            bank,
            updatedAccountType
        );

        // Handle IsEnabled status changes separately through the proper domain methods
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value && !partnerAccount.IsEnabled)
            {
                partnerAccount.Activate();
            }
            else if (!request.IsEnabled.Value && partnerAccount.IsEnabled)
            {
                partnerAccount.Disable();
            }
        }

        await _partnerAccountRepository.UpdatePartnerAccountAsync(partnerAccount, cancellationToken);

        return partnerAccount.Id.Value;
    }
}