using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdateBalance;

public class UpdateBalanceCommandHandler : ICommandHandler<UpdateBalanceCommand, Result<bool>>
{
    private readonly IPartnerAccountRepository _repo;

    public UpdateBalanceCommandHandler(IPartnerAccountRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(UpdateBalanceCommand cmd, CancellationToken ct)
    {
        var partnerAccount = await _repo.GetByIdAsync(PartnerAccountId.Of(cmd.PartnerAccountId), ct);
        if (partnerAccount is null)
            throw new BusinessException($"Partner account [{cmd.PartnerAccountId}] not found.");

        partnerAccount.UpdateBalance(cmd.NewBalance);

        _repo.Update(partnerAccount);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}