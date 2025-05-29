using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate.Exceptions;

namespace wfc.referential.Application.TaxRuleDetails.Commands.UpdateTaxRuleDetail;
public class UpdateTaxRuleDetailCommandHandler(
        ITaxRuleDetailRepository _taxRuleDetailsRepository,
        ICacheService _cacheService) : ICommandHandler<UpdateTaxRuleDetailCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(UpdateTaxRuleDetailCommand request, CancellationToken cancellationToken)
    {
        var taxRuleDetail = await _taxRuleDetailsRepository.GetTaxRuleDetailByIdAsync(
            TaxRuleDetailsId.Of(request.TaxRuleDetailsId).Value,
            cancellationToken);

        if (taxRuleDetail is null)
            throw new ResourceNotFoundException($"{nameof(TaxRuleDetail)} with ID {request.TaxRuleDetailsId} not found.");

        var existing = await _taxRuleDetailsRepository.GetByCorridorTaxServiceAsync(
            request.CorridorId, request.TaxId, request.ServiceId, cancellationToken);

        if (existing is not null && existing.Id != taxRuleDetail.Id)
            throw new TaxRuleDetailAlreadyExistException(existing.Id!.Value);

        request.Adapt(taxRuleDetail);

        taxRuleDetail.Update();

        await _taxRuleDetailsRepository.UpdateTaxRuleDetailAsync(taxRuleDetail, cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.TaxRuleDetail.Prefix, cancellationToken);

        return Result.Success(taxRuleDetail.Id!.Value);
    }
}