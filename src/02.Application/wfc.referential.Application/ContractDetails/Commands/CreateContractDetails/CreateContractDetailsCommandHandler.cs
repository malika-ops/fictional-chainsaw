using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.ContractDetailsAggregate.Exceptions;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Application.ContractDetails.Commands.CreateContractDetails;

public class CreateContractDetailsCommandHandler : ICommandHandler<CreateContractDetailsCommand, Result<Guid>>
{
    private readonly IContractDetailsRepository _repo;
    private readonly IContractRepository _contractRepository;
    private readonly IPricingRepository _pricingRepository;

    public CreateContractDetailsCommandHandler(
        IContractDetailsRepository repo,
        IContractRepository contractRepository,
        IPricingRepository pricingRepository)
    {
        _repo = repo;
        _contractRepository = contractRepository;
        _pricingRepository = pricingRepository;
    }

    public async Task<Result<Guid>> Handle(CreateContractDetailsCommand cmd, CancellationToken ct)
    {
        var contractId = ContractId.Of(cmd.ContractId);
        var pricingId = PricingId.Of(cmd.PricingId);

        var existing = await _repo.GetByConditionAsync(
            cd => cd.ContractId.Equals(contractId) && cd.PricingId.Equals(pricingId), ct);

        if (existing.Any())
            throw new ContractDetailsAlreadyExistException(cmd.ContractId, cmd.PricingId);

        // Validate Contract exists
        var contract = await _contractRepository.GetByIdAsync(contractId, ct);
        if (contract is null)
            throw new ResourceNotFoundException($"Contract with ID {cmd.ContractId} not found");

        // Validate Pricing exists  
        var pricing = await _pricingRepository.GetByIdAsync(pricingId, ct);
        if (pricing is null)
            throw new ResourceNotFoundException($"Pricing with ID {cmd.PricingId} not found");

        var id = ContractDetailsId.Of(Guid.NewGuid());
        var contractDetails = Domain.ContractDetailsAggregate.ContractDetails.Create(
            id,
            contractId,  
            pricingId); 

        await _repo.AddAsync(contractDetails, ct);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(contractDetails.Id.Value);
    }
}