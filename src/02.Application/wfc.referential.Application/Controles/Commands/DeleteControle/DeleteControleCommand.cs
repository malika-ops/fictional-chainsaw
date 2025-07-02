using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Controles.Commands.DeleteControle;

public record DeleteControleCommand : ICommand<Result<bool>>
{
    public Guid ControleId { get; init; }
}