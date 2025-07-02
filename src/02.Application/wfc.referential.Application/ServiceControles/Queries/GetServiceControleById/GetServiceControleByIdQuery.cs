using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.ServiceControles.Dtos;

namespace wfc.referential.Application.ServiceControles.Queries.GetServiceControleById;

public record GetServiceControleByIdQuery : IQuery<ServiceControleResponse>
{
    public Guid ServiceControleId { get; init; }
}