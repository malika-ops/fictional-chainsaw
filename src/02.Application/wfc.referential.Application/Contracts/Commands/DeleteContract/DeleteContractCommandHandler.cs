using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractAggregate.Exceptions;

namespace wfc.referential.Application.Contracts.Commands.DeleteContract;

public class DeleteContractCommandHandler : ICommandHandler<DeleteContractCommand, Result<bool>>
{
    private readonly IContractRepository _contractRepo;

    public DeleteContractCommandHandler(IContractRepository contractRepo)
    {
        _contractRepo = contractRepo;
    }

    public async Task<Result<bool>> Handle(DeleteContractCommand cmd, CancellationToken ct)
    {
        var contract = await _contractRepo.GetByIdAsync(ContractId.Of(cmd.ContractId), ct);
        if (contract is null)
            throw new InvalidContractDeletingException("Contract not found");

        // Disable the contract instead of physically deleting it
        contract.Disable();
        await _contractRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}