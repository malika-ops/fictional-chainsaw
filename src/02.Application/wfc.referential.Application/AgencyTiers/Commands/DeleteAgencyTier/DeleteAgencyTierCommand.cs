using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.AgencyTiers.Commands.DeleteAgencyTier;

public record DeleteAgencyTierCommand : ICommand<Result<bool>>
{
    public Guid AgencyTierId { get; init; }

}