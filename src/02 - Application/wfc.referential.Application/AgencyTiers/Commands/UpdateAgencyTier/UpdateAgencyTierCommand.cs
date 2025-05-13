using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.AgencyTiers.Commands.UpdateAgencyTier;

public record UpdateAgencyTierCommand : ICommand<Result<Guid>>
{
    public Guid AgencyTierId { get; init; }
    public Guid AgencyId { get; init; }
    public Guid TierId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
}
