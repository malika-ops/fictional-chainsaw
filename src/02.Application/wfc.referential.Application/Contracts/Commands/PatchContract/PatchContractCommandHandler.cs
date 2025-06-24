using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractAggregate.Exceptions;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Contracts.Commands.PatchContract;

public class PatchContractCommandHandler : ICommandHandler<PatchContractCommand, Result<bool>>
{
    private readonly IContractRepository _repo;
    private readonly IPartnerRepository _partnerRepository;

    public PatchContractCommandHandler(
        IContractRepository repo,
        IPartnerRepository partnerRepository)
    {
        _repo = repo;
        _partnerRepository = partnerRepository;
    }

    public async Task<Result<bool>> Handle(PatchContractCommand cmd, CancellationToken ct)
    {
        var contract = await _repo.GetByIdAsync(ContractId.Of(cmd.ContractId), ct);
        if (contract is null)
            throw new ResourceNotFoundException($"Contract [{cmd.ContractId}] not found.");

        // Check if code is unique (if being updated)
        if (cmd.Code != null && cmd.Code != contract.Code)
        {
            var existingByCode = await _repo.GetByConditionAsync(c => c.Code == cmd.Code, ct);
            if (existingByCode.Any())
                throw new ContractCodeAlreadyExistException(cmd.Code);
        }

        // Validate Partner exists if provided
        if (cmd.PartnerId.HasValue)
        {
            var partner = await _partnerRepository.GetByIdAsync(PartnerId.Of(cmd.PartnerId.Value), ct);
            if (partner is null)
                throw new ResourceNotFoundException($"Partner with ID {cmd.PartnerId.Value} not found");
        }

        contract.Patch(
            cmd.Code,
            cmd.PartnerId.HasValue ? PartnerId.Of(cmd.PartnerId.Value) : null,
            cmd.StartDate,
            cmd.EndDate,
            cmd.IsEnabled);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}