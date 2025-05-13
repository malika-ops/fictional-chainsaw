using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Commands.UpdateService;

public record UpdateServiceCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid ServiceId { get; init; }
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsEnabled { get; init; } = true;
    public ProductId ProductId { get; init; } = default!;

    public string CacheKey => $"{nameof(Service)}_{ServiceId}";
    public int CacheExpiration => 5;
}