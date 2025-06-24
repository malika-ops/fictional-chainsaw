using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.PatchTaxRuleDetail;

public class PatchTaxRuleDetailCommandHandler(
        ITaxRuleDetailRepository _taxRuleDetailsRepository,
        ICorridorRepository corridorRepository,
        ITaxRepository taxRepository,
        IServiceRepository serviceRepository) : ICommandHandler<PatchTaxRuleDetailCommand, Result<bool>>
{

    public async Task<Result<bool>> Handle(PatchTaxRuleDetailCommand request, CancellationToken cancellationToken)
    {
        var taxRuleDetailsId = TaxRuleDetailsId.Of(request.TaxRuleDetailsId);
        var taxRuleDetail = await _taxRuleDetailsRepository.GetByIdAsync(taxRuleDetailsId, cancellationToken);
        if (taxRuleDetail is null)
            throw new ResourceNotFoundException($"{nameof(TaxRuleDetail)} with ID {request.TaxRuleDetailsId} not found.");

        CorridorId? corridorId = null;
        if (request.CorridorId.HasValue)
        {
            corridorId = CorridorId.Of(request.CorridorId.Value);
            var corridor = await corridorRepository.GetByIdAsync(corridorId, cancellationToken);
            if (corridor is null) throw new ResourceNotFoundException(
                $"Corridor with ID {corridorId.Value} not found.");
        }

        TaxId? taxId = null;
        if (request.TaxId.HasValue)
        {
            taxId = TaxId.Of(request.TaxId.Value);
            var tax = await taxRepository.GetByIdAsync(taxId, cancellationToken);
            if (tax is null) throw new ResourceNotFoundException(
                $"Tax with ID {taxId.Value} not found.");
        }

        ServiceId? serviceId = null;
        if(request.ServiceId.HasValue) {
            serviceId = ServiceId.Of(request.ServiceId.Value);
            var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            if (service is null) throw new ResourceNotFoundException(
                $"Service with ID {serviceId.Value} not found.");
        }

        taxRuleDetail.Patch(taxRuleDetailsId, corridorId, taxId, serviceId, request.AppliedOn, request.IsEnabled);
        
        _taxRuleDetailsRepository.Update(taxRuleDetail);
        await _taxRuleDetailsRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}