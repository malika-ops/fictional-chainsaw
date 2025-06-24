using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.AffiliateAggregate.Exceptions;

namespace wfc.referential.Application.Affiliates.Commands.DeleteAffiliate;

public class DeleteAffiliateCommandHandler : ICommandHandler<DeleteAffiliateCommand, Result<bool>>
{
    private readonly IAffiliateRepository _affiliateRepo;
    private readonly IPricingRepository _pricingRepo;

    public DeleteAffiliateCommandHandler(
        IAffiliateRepository affiliateRepo,
        IPricingRepository pricingRepo)
    {
        _affiliateRepo = affiliateRepo;
        _pricingRepo = pricingRepo;
    }

    public async Task<Result<bool>> Handle(DeleteAffiliateCommand cmd, CancellationToken ct)
    {
        var affiliate = await _affiliateRepo.GetByIdAsync(AffiliateId.Of(cmd.AffiliateId), ct);
        if (affiliate is null)
            throw new InvalidAffiliateDeletingException($"Affiliate [{cmd.AffiliateId}] not found.");

        // Check if affiliate is linked to pricings using BaseRepository method
        var linkedPricings = await _pricingRepo.GetByConditionAsync(p => p.AffiliateId == affiliate.Id, ct);
        if (linkedPricings.Any())
            throw new AffiliateLinkedToPricingException(cmd.AffiliateId);

        // Disable the affiliate instead of physically deleting it
        affiliate.Disable();
        await _affiliateRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}