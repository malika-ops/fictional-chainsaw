using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;

public class DeleteTaxRuleDetailCommandHandler(
        ITaxRuleDetailRepository _taxRuleDetailsRepository,
        ICacheService _cacheService) : ICommandHandler<DeleteTaxRuleDetailCommand, Result<bool>>
{

    public async Task<Result<bool>> Handle(DeleteTaxRuleDetailCommand request, CancellationToken cancellationToken)
    {
        var taxRuleDetail = await _taxRuleDetailsRepository.GetTaxRuleDetailByIdAsync(
            TaxRuleDetailsId.Of(request.TaxRuleDetailsId).Value,
            cancellationToken);

        if (taxRuleDetail is null)
            throw new ResourceNotFoundException($"{nameof(TaxRuleDetail)} not found");

        taxRuleDetail.SetInactive();

        await _taxRuleDetailsRepository.UpdateTaxRuleDetailAsync(taxRuleDetail, cancellationToken);

        await _cacheService.SetAsync(
            request.CacheKey,
            taxRuleDetail,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken);

        return Result.Success(true);
    }
}