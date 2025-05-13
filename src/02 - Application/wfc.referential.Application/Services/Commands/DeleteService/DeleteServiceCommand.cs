using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Commands.DeleteService;

public record DeleteServiceCommand : ICommand<Result<bool>>, ICacheableQuery
{
    public Guid ServiceId { get; init; }
    public string CacheKey => $"{nameof(Service)}_{ServiceId}";
    public int CacheExpiration => 5;
}