using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.ContractDetailsAggregate.Exceptions;

namespace wfc.referential.Application.ContractDetails.Commands.DeleteContractDetails;

public class DeleteContractDetailsCommandHandler : ICommandHandler<DeleteContractDetailsCommand, Result<bool>>
{
    private readonly IContractDetailsRepository _repo;

    public DeleteContractDetailsCommandHandler(IContractDetailsRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(DeleteContractDetailsCommand cmd, CancellationToken ct)
    {
        var contractDetails = await _repo.GetByIdAsync(ContractDetailsId.Of(cmd.ContractDetailsId), ct);
        if (contractDetails is null)
            throw new ContractDetailsNotFoundException(cmd.ContractDetailsId);

        contractDetails.Disable();
        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}