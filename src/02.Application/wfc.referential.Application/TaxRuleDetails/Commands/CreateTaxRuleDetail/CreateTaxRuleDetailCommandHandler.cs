using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.CreateTaxRuleDetail;

public class CreateTaxRuleDetailCommandHandler(
        ITaxRuleDetailRepository _taxRuleDetailsRepository,
        ICorridorRepository corridorRepository,
        ITaxRepository taxRepository,
        IServiceRepository serviceRepository) 
    : ICommandHandler<CreateTaxRuleDetailCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(CreateTaxRuleDetailCommand request, CancellationToken cancellationToken)
    {
        var taxRuleDetailsId = TaxRuleDetailsId.Create();

        var corridorId = CorridorId.Of(request.CorridorId);
        var corridor = await corridorRepository.GetByIdAsync(corridorId, cancellationToken);
        if (corridor is null) throw new ResourceNotFoundException(
            $"Corridor with ID {corridorId.Value} not found.");

        var taxId = TaxId.Of(request.TaxId);
        var tax = await taxRepository.GetByIdAsync(taxId, cancellationToken);
        if (tax is null) throw new ResourceNotFoundException(
            $"Tax with ID {taxId.Value} not found.");

        var serviceId = ServiceId.Of(request.ServiceId);
        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service is null) throw new ResourceNotFoundException(
            $"Service with ID {serviceId.Value} not found.");

        var taxRuleDetails = TaxRuleDetail.Create(
            taxRuleDetailsId,
            corridorId,
            taxId,
            serviceId,
            request.AppliedOn,
            request.IsEnabled);

        await _taxRuleDetailsRepository.AddAsync(taxRuleDetails, cancellationToken);
        await _taxRuleDetailsRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(taxRuleDetails.Id!.Value);
    }
}