using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateBalance;

public class UpdateSupportAccountBalanceCommandHandler : ICommandHandler<UpdateSupportAccountBalanceCommand, Result<bool>>
{
    private readonly ISupportAccountRepository _repo;

    public UpdateSupportAccountBalanceCommandHandler(ISupportAccountRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(UpdateSupportAccountBalanceCommand cmd, CancellationToken ct)
    {
        var supportAccount = await _repo.GetByIdAsync(SupportAccountId.Of(cmd.SupportAccountId), ct);
        if (supportAccount is null)
            throw new BusinessException($"Support account [{cmd.SupportAccountId}] not found.");

        supportAccount.UpdateBalance(cmd.NewBalance);
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}