using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Controles.Commands.PatchControle;

public record PatchControleCommand : ICommand<Result<bool>>
{
    public Guid ControleId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
}