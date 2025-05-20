using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdateBalance;

public class UpdateBalanceCommandHandler : ICommandHandler<UpdateBalanceCommand, Guid>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;

    public UpdateBalanceCommandHandler(IPartnerAccountRepository partnerAccountRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
    }

    public async Task<Guid> Handle(UpdateBalanceCommand request, CancellationToken cancellationToken)
    {
        // Check if partner account exists
        var partnerAccount = await _partnerAccountRepository.GetByIdAsync(new PartnerAccountId(request.PartnerAccountId), cancellationToken);
        if (partnerAccount is null)
            throw new BusinessException($"Partner account with ID {request.PartnerAccountId} not found");

        // Update the balance using dedicated domain method
        partnerAccount.UpdateBalance(request.NewBalance);

        await _partnerAccountRepository.UpdatePartnerAccountAsync(partnerAccount, cancellationToken);

        return partnerAccount.Id.Value;
    }
}