using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;

public class UpdatePartnerAccountCommandHandler : ICommandHandler<UpdatePartnerAccountCommand, Result<bool>>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly IBankRepository _bankRepository;

    public UpdatePartnerAccountCommandHandler(
        IPartnerAccountRepository partnerAccountRepository,
        IBankRepository bankRepository
        )
    {
        _partnerAccountRepository = partnerAccountRepository;
        _bankRepository = bankRepository;
    }

    public async Task<Result<bool>> Handle(UpdatePartnerAccountCommand cmd, CancellationToken ct)
    {
        var partnerAccount = await _partnerAccountRepository.GetByIdAsync(PartnerAccountId.Of(cmd.PartnerAccountId), ct);
        if (partnerAccount is null)
            throw new BusinessException($"Partner account with ID {cmd.PartnerAccountId} not found");

        // Check uniqueness on AccountNumber (if changed)
        if (cmd.AccountNumber != partnerAccount.AccountNumber)
        {
            var duplicateAccountNumber = await _partnerAccountRepository.GetOneByConditionAsync(p => p.AccountNumber == cmd.AccountNumber, ct);
            if (duplicateAccountNumber is not null && duplicateAccountNumber.Id != partnerAccount.Id)
                throw new PartnerAccountAlreadyExistException(cmd.AccountNumber);
        }

        // Check uniqueness on RIB (if changed)
        if (cmd.RIB != partnerAccount.RIB)
        {
            var duplicateRIB = await _partnerAccountRepository.GetOneByConditionAsync(p => p.RIB == cmd.RIB, ct);
            if (duplicateRIB is not null && duplicateRIB.Id != partnerAccount.Id)
                throw new PartnerAccountAlreadyExistException(cmd.RIB);
        }

        // Validate Bank exists
        var bank = await _bankRepository.GetByIdAsync(BankId.Of(cmd.BankId), ct);
        if (bank is null)
            throw new BusinessException($"Bank with ID {cmd.BankId} not found");

        partnerAccount.Update(
            cmd.AccountNumber,
            cmd.RIB,
            cmd.Domiciliation,
            cmd.BusinessName,
            cmd.ShortName,
            cmd.AccountBalance,
            bank,
            cmd.PartnerAccountType);

        // Handle enabled status through domain methods
        if (cmd.IsEnabled && !partnerAccount.IsEnabled)
        {
            partnerAccount.Activate();
        }
        else if (!cmd.IsEnabled && partnerAccount.IsEnabled)
        {
            partnerAccount.Disable();
        }

        _partnerAccountRepository.Update(partnerAccount);
        await _partnerAccountRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}