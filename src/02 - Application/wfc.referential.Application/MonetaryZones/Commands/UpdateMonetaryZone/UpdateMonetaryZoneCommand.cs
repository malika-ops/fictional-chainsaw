using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;

public record UpdateMonetaryZoneCommand(Guid MonetaryZoneId, string Code, string Name , string Description , bool IsEnabled) : ICommand<Result<Guid>>
{
    public string CacheKey => $"{nameof(MonetaryZone)}_{MonetaryZoneId}";
    public int CacheExpiration => 5;

}
