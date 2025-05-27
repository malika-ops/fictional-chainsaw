using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Tiers.Commands.PatchTier;

public record PatchTierCommand : ICommand<Result<bool>>
{
    public Guid TierId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }

}