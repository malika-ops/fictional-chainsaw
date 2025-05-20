using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;

public class DeletePartnerAccountCommandHandler : ICommandHandler<DeletePartnerAccountCommand, bool>
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

    public async Task<bool> Handle(DeletePartnerAccountCommand request, CancellationToken cancellationToken)
    {
        var partnerAccount = await _partnerAccountRepository.GetByIdAsync(
            PartnerAccountId.Of(request.PartnerAccountId),
            cancellationToken);

        if (partnerAccount == null)
            throw new InvalidPartnerAccountDeletingException("Partner account not found");

        // Check if this account is a commission account attached to any partner
        var partners = await _partnerRepository.GetAllPartnersAsync(cancellationToken);
        var linkedPartner = partners.FirstOrDefault(p =>
            p.CommissionAccountId == request.PartnerAccountId);

        if (linkedPartner != null)
        {
            throw new BusinessException(
                $"This commission account is attached to partner '{linkedPartner.Label}'. " +
                "Please update the partner to remove this account reference before deleting.");
        }

        // Disable the partner account instead of physically deleting it
        partnerAccount.Disable();

        await _partnerAccountRepository.UpdatePartnerAccountAsync(partnerAccount, cancellationToken);

        return true;
    }
}