using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Operators.Dtos;

namespace wfc.referential.Application.Operators.Queries.GetOperatorById;

public record GetOperatorByIdQuery : IQuery<GetOperatorsResponse>
{
    public Guid OperatorId { get; init; }
}
