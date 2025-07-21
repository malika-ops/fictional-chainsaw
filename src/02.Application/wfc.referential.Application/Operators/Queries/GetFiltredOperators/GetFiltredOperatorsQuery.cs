using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Operators.Dtos;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Queries.GetFiltredOperators;

public record GetFiltredOperatorsQuery : IQuery<PagedResult<GetOperatorsResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Code { get; init; }
    public string? IdentityCode { get; init; }
    public string? LastName { get; init; }
    public string? FirstName { get; init; }
    public string? Email { get; init; }
    public OperatorType? OperatorType { get; init; }
    public Guid? BranchId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}