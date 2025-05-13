using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.CreateTaxRuleDetail;

public class CreateTaxRuleDetailCommandHandler(
        ITaxRuleDetailRepository _taxRuleDetailsRepository,
        ICacheService _cacheService) : ICommandHandler<CreateTaxRuleDetailCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(CreateTaxRuleDetailCommand request, CancellationToken cancellationToken)
    {
        var taxRuleDetails = TaxRuleDetail.Create(
            request.TaxRuleDetailsId,
            request.CorridorId,
            request.TaxId,
            request.ServiceId,
            request.AppliedOn,
            request.IsEnabled);

        await _taxRuleDetailsRepository.AddTaxRuleDetailAsync(taxRuleDetails, cancellationToken);

        await _cacheService.SetAsync(
            request.CacheKey,
            taxRuleDetails,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken);

        return Result.Success(taxRuleDetails.Id!.Value);
    }
}