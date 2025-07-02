using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Controles.Commands.UpdateControle;

public record UpdateControleCommand : ICommand<Result<bool>>
{
    public Guid ControleId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
}