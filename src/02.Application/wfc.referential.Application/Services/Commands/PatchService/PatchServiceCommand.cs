using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Commands.PatchService;

public record PatchServiceCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid ServiceId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
    public string CacheKey => $"{nameof(Service)}_{ServiceId}";
    public int CacheExpiration => 5;
}