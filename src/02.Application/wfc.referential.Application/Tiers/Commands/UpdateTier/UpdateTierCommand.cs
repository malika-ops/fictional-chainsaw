using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Tiers.Commands.UpdateTier;

public record UpdateTierCommand : ICommand<Result<bool>>
{
    public Guid TierId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;

}