using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

namespace wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;

public class DeletePartnerAccountCommandHandler : ICommandHandler<DeletePartnerAccountCommand, Result<bool>>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly IPartnerRepository _partnerRepository;

    public DeletePartnerAccountCommandHandler(
        IPartnerAccountRepository partnerAccountRepository,
        IPartnerRepository partnerRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
        _partnerRepository = partnerRepository;
    }

    public async Task<Result<bool>> Handle(DeletePartnerAccountCommand cmd, CancellationToken ct)
    {
        var partnerAccount = await _partnerAccountRepository.GetByIdAsync(PartnerAccountId.Of(cmd.PartnerAccountId), ct);
        if (partnerAccount is null)
            throw new BusinessException($"Partner account [{cmd.PartnerAccountId}] not found.");

        // Check if this account is linked to partners
        var linkedPartner = await _partnerRepository.GetOneByConditionAsync(p => p.CommissionAccountId == cmd.PartnerAccountId, ct);
        if (linkedPartner is not null)
            throw new PartnerAccountLinkedToTransactionsException(cmd.PartnerAccountId);

        partnerAccount.Disable();
        await _partnerAccountRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}