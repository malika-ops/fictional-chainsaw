using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;

public class DeleteSupportAccountCommandHandler : ICommandHandler<DeleteSupportAccountCommand, Result<bool>>
{
    private readonly ISupportAccountRepository _repo;

    public DeleteSupportAccountCommandHandler(ISupportAccountRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeleteSupportAccountCommand cmd, CancellationToken ct)
    {
        var supportAccount = await _repo.GetByIdAsync(SupportAccountId.Of(cmd.SupportAccountId), ct);
        if (supportAccount is null)
            throw new BusinessException($"Support account [{cmd.SupportAccountId}] not found.");

        supportAccount.Disable();
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}