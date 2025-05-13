using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;

public record DeleteMonetaryZoneCommand(Guid MonetaryZoneId) : ICommand<Result<bool>>
{
    public string CacheKey => $"{nameof(MonetaryZone)}_{MonetaryZoneId}";
    public int CacheExpiration => 5;
}
