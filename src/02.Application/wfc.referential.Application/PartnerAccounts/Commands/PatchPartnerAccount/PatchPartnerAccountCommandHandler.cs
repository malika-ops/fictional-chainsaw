using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

namespace wfc.referential.Application.PartnerAccounts.Commands.PatchPartnerAccount;

public class PatchPartnerAccountCommandHandler : ICommandHandler<PatchPartnerAccountCommand, Result<bool>>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly IBankRepository _bankRepository;

    public PatchPartnerAccountCommandHandler(
        IPartnerAccountRepository partnerAccountRepository,
        IBankRepository bankRepository
        )
    {
        _partnerAccountRepository = partnerAccountRepository;
        _bankRepository = bankRepository;
    }

    public async Task<Result<bool>> Handle(PatchPartnerAccountCommand cmd, CancellationToken ct)
    {
        var partnerAccount = await _partnerAccountRepository.GetByIdAsync(PartnerAccountId.Of(cmd.PartnerAccountId), ct);
        if (partnerAccount is null)
            throw new ResourceNotFoundException($"Partner account [{cmd.PartnerAccountId}] not found.");

        // Check uniqueness on AccountNumber (if provided and changed)
        if (!string.IsNullOrWhiteSpace(cmd.AccountNumber) && cmd.AccountNumber != partnerAccount.AccountNumber)
        {
            var duplicate = await _partnerAccountRepository.GetOneByConditionAsync(p => p.AccountNumber == cmd.AccountNumber, ct);
            if (duplicate is not null && duplicate.Id != partnerAccount.Id)
                throw new PartnerAccountAlreadyExistException(cmd.AccountNumber);
        }

        // Check uniqueness on RIB (if provided and changed)
        if (!string.IsNullOrWhiteSpace(cmd.RIB) && cmd.RIB != partnerAccount.RIB)
        {
            var duplicate = await _partnerAccountRepository.GetOneByConditionAsync(p => p.RIB == cmd.RIB, ct);
            if (duplicate is not null && duplicate.Id != partnerAccount.Id)
                throw new PartnerAccountAlreadyExistException(cmd.RIB);
        }

        // Get updated bank if needed
        Bank? bank = null;
        if (cmd.BankId.HasValue)
        {
            bank = await _bankRepository.GetByIdAsync(BankId.Of(cmd.BankId.Value), ct);
            if (bank is null)
                throw new BusinessException($"Bank with ID {cmd.BankId} not found");
        }

        partnerAccount.Patch(
            cmd.AccountNumber,
            cmd.RIB,
            cmd.Domiciliation,
            cmd.BusinessName,
            cmd.ShortName,
            cmd.AccountBalance,
            bank,
            cmd.PartnerAccountType,
            cmd.IsEnabled);

        _partnerAccountRepository.Update(partnerAccount);
        await _partnerAccountRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}