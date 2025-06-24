using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate.Exceptions;

namespace wfc.referential.Application.TaxRuleDetails.Commands.UpdateTaxRuleDetail;
public class UpdateTaxRuleDetailCommandHandler(
        ITaxRuleDetailRepository _taxRuleDetailsRepository,
        ICorridorRepository corridorRepository,
        ITaxRepository taxRepository,
        IServiceRepository serviceRepository) : ICommandHandler<UpdateTaxRuleDetailCommand, Result<bool>>
{

    public async Task<Result<bool>> Handle(UpdateTaxRuleDetailCommand request, CancellationToken cancellationToken)
    {
        var taxRuleDetailsId = TaxRuleDetailsId.Of(request.TaxRuleDetailsId);
        var taxRuleDetail = await _taxRuleDetailsRepository.GetByIdAsync(taxRuleDetailsId, cancellationToken);
        if (taxRuleDetail is null)
            throw new ResourceNotFoundException($"{nameof(TaxRuleDetail)} with ID {request.TaxRuleDetailsId} not found.");

        var corridorId = CorridorId.Of(request.CorridorId);
        var corridor = await corridorRepository.GetByIdAsync(corridorId, cancellationToken);
        if (corridor is null) throw new ResourceNotFoundException($"Corridor with ID {corridorId.Value} not found.");

        var taxId = TaxId.Of(request.TaxId);
        var tax = await taxRepository.GetByIdAsync(taxId, cancellationToken);
        if (tax is null) throw new ResourceNotFoundException($"Tax with ID {taxId.Value} not found.");

        var serviceId = ServiceId.Of(request.ServiceId);
        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service is null) throw new ResourceNotFoundException($"Service with ID {serviceId.Value} not found.");
        

        var existing = await _taxRuleDetailsRepository.GetOneByConditionAsync( r => r.CorridorId == corridorId && 
        r.TaxId == taxId && r.ServiceId == serviceId , cancellationToken);

        if (existing is not null && existing.Id != taxRuleDetail.Id)
            throw new TaxRuleDetailAlreadyExistException(existing.Id!.Value);

        taxRuleDetail.Update(taxRuleDetailsId, corridorId, taxId, serviceId, request.AppliedOn, request.IsEnabled);

        _taxRuleDetailsRepository.Update(taxRuleDetail);
        await _taxRuleDetailsRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}