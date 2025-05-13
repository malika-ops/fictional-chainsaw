using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Tiers.Commands.DeleteTier;

public record DeleteTierCommand : ICommand<Result<bool>>
{
    public Guid TierId { get; init; }
}