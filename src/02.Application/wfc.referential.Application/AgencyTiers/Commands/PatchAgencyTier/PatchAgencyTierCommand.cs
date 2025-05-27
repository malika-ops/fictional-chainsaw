using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.AgencyTiers.Commands.PatchAgencyTier;

public record PatchAgencyTierCommand : ICommand<Result<bool>>
{
    public Guid AgencyTierId { get; init; }
    public Guid? AgencyId { get; init; }
    public Guid? TierId { get; init; }
    public string? Code { get; init; }
    public string? Password { get; init; }
    public bool? IsEnabled { get; init; }
}