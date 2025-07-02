using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ServiceControles.Commands.DeleteServiceControle;

public record DeleteServiceControleCommand : ICommand<Result<bool>>
{
    public Guid ServiceControleId { get; init; }
}