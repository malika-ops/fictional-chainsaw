using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;

namespace wfc.referential.Application.Partners.Commands.DeletePartner;

public record DeletePartnerCommandHandler : ICommandHandler<DeletePartnerCommand, bool>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly ISupportAccountRepository _supportAccountRepository;

    public DeletePartnerCommandHandler(
        IPartnerRepository partnerRepository,
        ISupportAccountRepository supportAccountRepository)
    {
        _partnerRepository = partnerRepository;
        _supportAccountRepository = supportAccountRepository;
    }

    public async Task<bool> Handle(DeletePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await _partnerRepository.GetByIdAsync(PartnerId.Of(request.PartnerId), cancellationToken);

        if (partner == null)
            throw new InvalidPartnerDeletingException("Partner not found");

        // Check if this partner has any support accounts
        var supportAccounts = await _supportAccountRepository.GetByPartnerIdAsync(request.PartnerId, cancellationToken);
        if (supportAccounts.Any())
            throw new InvalidPartnerDeletingException("Cannot delete partner with existing support accounts");

        // Disable the partner instead of physically deleting it
        partner.Disable();

        await _partnerRepository.UpdatePartnerAsync(partner, cancellationToken);

        return true;
    }
}