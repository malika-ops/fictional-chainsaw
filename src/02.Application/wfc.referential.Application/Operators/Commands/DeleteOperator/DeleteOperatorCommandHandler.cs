using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.OperatorAggregate;
using wfc.referential.Domain.OperatorAggregate.Exceptions;

namespace wfc.referential.Application.Operators.Commands.DeleteOperator;

public class DeleteOperatorCommandHandler : ICommandHandler<DeleteOperatorCommand, Result<bool>>
{
    private readonly IOperatorRepository _operatorRepo;

    public DeleteOperatorCommandHandler(IOperatorRepository operatorRepo)
    {
        _operatorRepo = operatorRepo;
    }

    public async Task<Result<bool>> Handle(DeleteOperatorCommand cmd, CancellationToken ct)
    {
        var operatorEntity = await _operatorRepo.GetByIdAsync(OperatorId.Of(cmd.OperatorId), ct);
        if (operatorEntity is null)
            throw new InvalidOperatorDeletingException($"Operator [{cmd.OperatorId}] not found.");

        // Disable the operator instead of physically deleting it
        operatorEntity.Disable();
        await _operatorRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}