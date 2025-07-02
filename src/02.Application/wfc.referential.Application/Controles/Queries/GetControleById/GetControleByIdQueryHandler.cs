using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Controles.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;

namespace wfc.referential.Application.Controles.Queries.GetControleById;

public class GetControleByIdQueryHandler : IQueryHandler<GetControleByIdQuery, GetControleResponse>
{
    private readonly IControleRepository _controleRepo;

    public GetControleByIdQueryHandler(IControleRepository controleRepo)
    {
        _controleRepo = controleRepo;
    }

    public async Task<GetControleResponse> Handle(GetControleByIdQuery query, CancellationToken ct)
    {
        var id = ControleId.Of(query.ControleId);
        var entity = await _controleRepo.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Controle with id '{query.ControleId}' not found.");

        return entity.Adapt<GetControleResponse>();
    }
}