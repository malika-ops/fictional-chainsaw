using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.ContractDetailsAggregate.Exceptions;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Application.ContractDetails.Commands.UpdateContractDetails;

public class UpdateContractDetailsCommandHandler : ICommandHandler<UpdateContractDetailsCommand, Result<bool>>
{
    private readonly IContractDetailsRepository _repo;
    private readonly IContractRepository _contractRepository;
    private readonly IPricingRepository _pricingRepository;

    public UpdateContractDetailsCommandHandler(
        IContractDetailsRepository repo,
        IContractRepository contractRepository,
        IPricingRepository pricingRepository)
    {
        _repo = repo;
        _contractRepository = contractRepository;
        _pricingRepository = pricingRepository;
    }

    public async Task<Result<bool>> Handle(UpdateContractDetailsCommand cmd, CancellationToken ct)
    {
        var contractDetails = await _repo.GetByIdAsync(ContractDetailsId.Of(cmd.ContractDetailsId), ct);
        if (contractDetails is null)
            throw new ResourceNotFoundException($"ContractDetails [{cmd.ContractDetailsId}] not found.");

        // Check if the new combination already exists (if changed)
        if (cmd.ContractId != contractDetails.ContractId.Value || cmd.PricingId != contractDetails.PricingId.Value)
        {
            var existing = await _repo.GetByConditionAsync(
                cd => cd.ContractId.Value == cmd.ContractId &&
                      cd.PricingId.Value == cmd.PricingId &&
                      cd.Id.Value != cmd.ContractDetailsId, ct);
            if (existing.Any())
                throw new ContractDetailsAlreadyExistException(cmd.ContractId, cmd.PricingId);
        }

        // Validate Contract exists
        var contract = await _contractRepository.GetByIdAsync(ContractId.Of(cmd.ContractId), ct);
        if (contract is null)
            throw new ResourceNotFoundException($"Contract with ID {cmd.ContractId} not found");

        // Validate Pricing exists
        var pricing = await _pricingRepository.GetByIdAsync(PricingId.Of(cmd.PricingId), ct);
        if (pricing is null)
            throw new ResourceNotFoundException($"Pricing with ID {cmd.PricingId} not found");

        contractDetails.Update(
            ContractId.Of(cmd.ContractId),
            PricingId.Of(cmd.PricingId),
            cmd.IsEnabled);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}