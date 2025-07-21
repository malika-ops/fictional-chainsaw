using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Operators.Commands.DeleteOperator;

public record DeleteOperatorCommand : ICommand<Result<bool>>
{
    public Guid OperatorId { get; init; }
}