using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Controles.Dtos;

namespace wfc.referential.Application.Controles.Queries.GetControleById;

public record GetControleByIdQuery : IQuery<GetControleResponse>
{
    public Guid ControleId { get; init; }
}