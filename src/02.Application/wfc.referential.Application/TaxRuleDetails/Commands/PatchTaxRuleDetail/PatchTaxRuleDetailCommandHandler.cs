using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.PatchTaxRuleDetail;

public class PatchTaxRuleDetailCommandHandler(
        ITaxRuleDetailRepository _taxRuleDetailsRepository,
        ICacheService _cacheService) : ICommandHandler<PatchTaxRuleDetailCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(PatchTaxRuleDetailCommand request, CancellationToken cancellationToken)
    {
        var taxRuleDetail = await _taxRuleDetailsRepository.GetTaxRuleDetailByIdAsync(
            TaxRuleDetailsId.Of(request.TaxRuleDetailsId).Value,
            cancellationToken);

        if (taxRuleDetail is null)
            throw new ResourceNotFoundException($"{nameof(TaxRuleDetail)} with ID {request.TaxRuleDetailsId} not found.");

        request.Adapt(taxRuleDetail);


        taxRuleDetail.Patch();
        await _taxRuleDetailsRepository.UpdateTaxRuleDetailAsync(taxRuleDetail, cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.TaxRuleDetail.Prefix, cancellationToken);

        return Result.Success(taxRuleDetail.Id!.Value);
    }
}