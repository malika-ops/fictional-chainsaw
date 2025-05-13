using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Agencies.Commands.DeleteAgency;

public record DeleteAgencyCommand : ICommand<Result<bool>>
{
    public Guid AgencyId { get; }

    public DeleteAgencyCommand(Guid agencyId) => AgencyId = agencyId;
}