using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.ServiceControles.Dtos;

namespace wfc.referential.Application.ServiceControles.Queries.GetFiltredServiceControles;

public record GetFiltredServiceControlesQuery : IQuery<PagedResult<ServiceControleResponse>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public Guid? ServiceId { get; init; }
    public Guid? ControleId { get; init; }
    public Guid? ChannelId { get; init; }
    public bool? IsEnabled { get; init; }
}