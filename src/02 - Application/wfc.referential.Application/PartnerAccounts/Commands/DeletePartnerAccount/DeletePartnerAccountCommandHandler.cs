using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

namespace wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;

public class DeletePartnerAccountCommandHandler : ICommandHandler<DeletePartnerAccountCommand, bool>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;

    public DeletePartnerAccountCommandHandler(IPartnerAccountRepository partnerAccountRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
    }

    public async Task<bool> Handle(DeletePartnerAccountCommand request, CancellationToken cancellationToken)
    {
        var partnerAccount = await _partnerAccountRepository.GetByIdAsync(PartnerAccountId.Of(request.PartnerAccountId), cancellationToken);

        if (partnerAccount == null)
            throw new InvalidPartnerAccountDeletingException("Partner account not found");

        // Disable the partner account instead of physically deleting it
        partnerAccount.Disable();

        await _partnerAccountRepository.UpdatePartnerAccountAsync(partnerAccount, cancellationToken);

        return true;
    }
}