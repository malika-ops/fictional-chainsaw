using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;

public record CreateMonetaryZoneCommand(string Code, string Name, string Description) : ICommand<Result<Guid>>
{
    public string CacheKey => $"{nameof(MonetaryZone)}_{Code}";
    public int CacheExpiration => 5;

}
