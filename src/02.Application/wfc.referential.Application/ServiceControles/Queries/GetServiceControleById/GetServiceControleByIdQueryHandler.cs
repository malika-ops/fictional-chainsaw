using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ServiceControles.Dtos;
using wfc.referential.Domain.ServiceControleAggregate;

namespace wfc.referential.Application.ServiceControles.Queries.GetServiceControleById;

public class GetServiceControleByIdQueryHandler 
    : IQueryHandler<GetServiceControleByIdQuery, ServiceControleResponse>
{
    private readonly IServiceControleRepository _repo;

    public GetServiceControleByIdQueryHandler(IServiceControleRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceControleResponse> Handle(
        GetServiceControleByIdQuery q,
        CancellationToken ct)
    {
        var id = ServiceControleId.Of(q.ServiceControleId);

        var entity = await _repo.GetByIdWithIncludesAsync(
            id,
            ct,
            sc => sc.Channel!
        ) ?? throw new ResourceNotFoundException(
                $"ServiceControle with id '{q.ServiceControleId}' not found.");

        return entity.Adapt<ServiceControleResponse>();
    }
}