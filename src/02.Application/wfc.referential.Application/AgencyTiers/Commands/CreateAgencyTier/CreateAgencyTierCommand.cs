using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.AgencyTiers.Commands.CreateAgencyTier;

public record CreateAgencyTierCommand : ICommand<Result<Guid>>
{
    public Guid AgencyId { get; init; }
    public Guid TierId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Password { get; init; }
}