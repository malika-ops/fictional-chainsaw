using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Operators.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Queries.GetOperatorById;

public class GetOperatorByIdQueryHandler : IQueryHandler<GetOperatorByIdQuery, GetOperatorsResponse>
{
    private readonly IOperatorRepository _operatorRepository;

    public GetOperatorByIdQueryHandler(IOperatorRepository operatorRepository)
    {
        _operatorRepository = operatorRepository;
    }

    public async Task<GetOperatorsResponse> Handle(GetOperatorByIdQuery query, CancellationToken ct)
    {
        var id = OperatorId.Of(query.OperatorId);
        var entity = await _operatorRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Operator with id '{query.OperatorId}' not found.");

        return entity.Adapt<GetOperatorsResponse>();
    }
}