using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Corridors.Commands.DeleteCorridor;

public record DeleteCorridorCommand : ICommand<Result<bool>>
{
    public Guid CorridorId { get; init; }
}