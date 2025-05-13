using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Corridors.Commands.DeleteCorridor;

public record DeleteCorridorCommand : ICommand<Result<bool>>, ICacheableQuery
{
    public Guid CorridorId { get; init; }
    public string CacheKey => $"{nameof(Corridor)}_{CorridorId}";
    public int CacheExpiration => 5;
}