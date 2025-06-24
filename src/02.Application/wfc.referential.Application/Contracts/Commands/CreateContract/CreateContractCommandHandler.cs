using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractAggregate.Exceptions;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Contracts.Commands.CreateContract;

public class CreateContractCommandHandler : ICommandHandler<CreateContractCommand, Result<Guid>>
{
    private readonly IContractRepository _repo;
    private readonly IPartnerRepository _partnerRepository;

    public CreateContractCommandHandler(
        IContractRepository repo,
        IPartnerRepository partnerRepository)
    {
        _repo = repo;
        _partnerRepository = partnerRepository;
    }

    public async Task<Result<Guid>> Handle(CreateContractCommand cmd, CancellationToken ct)
    {
        // Check if the code already exists
        var existingByCode = await _repo.GetByConditionAsync(c => c.Code == cmd.Code, ct);
        if (existingByCode.Any())
            throw new ContractCodeAlreadyExistException(cmd.Code);

        // Validate Partner exists
        var partner = await _partnerRepository.GetByIdAsync(PartnerId.Of(cmd.PartnerId), ct);
        if (partner is null)
            throw new ResourceNotFoundException($"Partner with ID {cmd.PartnerId} not found");

        var id = ContractId.Of(Guid.NewGuid());
        var contract = Contract.Create(
            id,
            cmd.Code,
            PartnerId.Of(cmd.PartnerId),
            cmd.StartDate,
            cmd.EndDate);

        await _repo.AddAsync(contract, ct);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(contract.Id.Value);
    }
}