using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.ContractDetailsAggregate.Exceptions;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Application.ContractDetails.Commands.PatchContractDetails;

public class PatchContractDetailsCommandHandler : ICommandHandler<PatchContractDetailsCommand, Result<bool>>
{
    private readonly IContractDetailsRepository _repo;
    private readonly IContractRepository _contractRepository;
    private readonly IPricingRepository _pricingRepository;

    public PatchContractDetailsCommandHandler(
        IContractDetailsRepository repo,
        IContractRepository contractRepository,
        IPricingRepository pricingRepository)
    {
        _repo = repo;
        _contractRepository = contractRepository;
        _pricingRepository = pricingRepository;
    }

    public async Task<Result<bool>> Handle(PatchContractDetailsCommand cmd, CancellationToken ct)
    {
        var contractDetails = await _repo.GetByIdAsync(ContractDetailsId.Of(cmd.ContractDetailsId), ct);
        if (contractDetails is null)
            throw new ResourceNotFoundException($"ContractDetails [{cmd.ContractDetailsId}] not found.");

        // Determine final values after patching
        var finalContractId = cmd.ContractId ?? contractDetails.ContractId.Value;
        var finalPricingId = cmd.PricingId ?? contractDetails.PricingId.Value;

        // Check if the new combination already exists (if changed)
        if ((cmd.ContractId.HasValue && cmd.ContractId.Value != contractDetails.ContractId.Value) ||
            (cmd.PricingId.HasValue && cmd.PricingId.Value != contractDetails.PricingId.Value))
        {
            var existing = await _repo.GetByConditionAsync(
                cd => cd.ContractId.Value == finalContractId &&
                      cd.PricingId.Value == finalPricingId &&
                      cd.Id.Value != cmd.ContractDetailsId, ct);
            if (existing.Any())
                throw new ContractDetailsAlreadyExistException(finalContractId, finalPricingId);
        }

        // Validate Contract exists if provided
        if (cmd.ContractId.HasValue)
        {
            var contract = await _contractRepository.GetByIdAsync(ContractId.Of(cmd.ContractId.Value), ct);
            if (contract is null)
                throw new ResourceNotFoundException($"Contract with ID {cmd.ContractId.Value} not found");
        }

        // Validate Pricing exists if provided
        if (cmd.PricingId.HasValue)
        {
            var pricing = await _pricingRepository.GetByIdAsync(PricingId.Of(cmd.PricingId.Value), ct);
            if (pricing is null)
                throw new ResourceNotFoundException($"Pricing with ID {cmd.PricingId.Value} not found");
        }

        contractDetails.Patch(
            cmd.ContractId.HasValue ? ContractId.Of(cmd.ContractId.Value) : null,
            cmd.PricingId.HasValue ? PricingId.Of(cmd.PricingId.Value) : null,
            cmd.IsEnabled);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}