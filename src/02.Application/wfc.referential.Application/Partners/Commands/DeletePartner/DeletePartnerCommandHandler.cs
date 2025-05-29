using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;

namespace wfc.referential.Application.Partners.Commands.DeletePartner;

public class DeletePartnerCommandHandler : ICommandHandler<DeletePartnerCommand, Result<bool>>
{
    private readonly IPartnerRepository _partnerRepo;
    private readonly ISupportAccountRepository _supportAccountRepo;

    public DeletePartnerCommandHandler(
        IPartnerRepository partnerRepo,
        ISupportAccountRepository supportAccountRepo)
    {
        _partnerRepo = partnerRepo;
        _supportAccountRepo = supportAccountRepo;
    }

    public async Task<Result<bool>> Handle(DeletePartnerCommand cmd, CancellationToken ct)
    {
        var partner = await _partnerRepo.GetByIdAsync(PartnerId.Of(cmd.PartnerId), ct);
        if (partner is null)
            throw new InvalidPartnerDeletingException("Partner not found");

        // Check if this partner has any support accounts
        var supportAccounts = await _supportAccountRepo.GetByConditionAsync(
            sa => sa.PartnerId != null && sa.PartnerId.Value == cmd.PartnerId, ct);
        if (supportAccounts.Any())
            throw new InvalidPartnerDeletingException("Cannot delete partner with existing support accounts");

        // Disable the partner instead of physically deleting it
        partner.Disable();
        await _partnerRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}