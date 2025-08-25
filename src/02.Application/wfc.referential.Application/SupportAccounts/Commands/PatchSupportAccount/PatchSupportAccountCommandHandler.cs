using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate.Exceptions;

namespace wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;

public class PatchSupportAccountCommandHandler : ICommandHandler<PatchSupportAccountCommand, Result<bool>>
{
    private readonly ISupportAccountRepository _repo;

    public PatchSupportAccountCommandHandler(ISupportAccountRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(PatchSupportAccountCommand cmd, CancellationToken ct)
    {
        var supportAccount = await _repo.GetByIdAsync(SupportAccountId.Of(cmd.SupportAccountId), ct);
        if (supportAccount is null)
            throw new ResourceNotFoundException($"Support account [{cmd.SupportAccountId}] not found.");

        // Check uniqueness on Code (if provided)
        if (!string.IsNullOrWhiteSpace(cmd.Code))
        {
            var duplicateCode = await _repo.GetOneByConditionAsync(sa => sa.Code == cmd.Code, ct);
            if (duplicateCode is not null && duplicateCode.Id != supportAccount.Id)
                throw new SupportAccountCodeAlreadyExistException(cmd.Code);
        }

        // Check uniqueness on AccountingNumber (if provided)
        if (!string.IsNullOrWhiteSpace(cmd.AccountingNumber))
        {
            var duplicateAccountingNumber = await _repo.GetOneByConditionAsync(sa => sa.AccountingNumber == cmd.AccountingNumber, ct);
            if (duplicateAccountingNumber is not null && duplicateAccountingNumber.Id != supportAccount.Id)
                throw new SupportAccountAccountingNumberAlreadyExistException(cmd.AccountingNumber);
        }

        supportAccount.Patch(
            cmd.Code,
            cmd.Description,
            cmd.Threshold,
            cmd.Limit,
            cmd.AccountBalance,
            cmd.AccountingNumber,
            cmd.SupportAccountType,
            cmd.IsEnabled);

        _repo.Update(supportAccount);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
