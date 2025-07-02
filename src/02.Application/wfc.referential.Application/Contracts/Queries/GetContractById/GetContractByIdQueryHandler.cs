using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Contracts.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractAggregate;

namespace wfc.referential.Application.Contracts.Queries.GetContractById;

public class GetContractByIdQueryHandler : IQueryHandler<GetContractByIdQuery, GetContractsResponse>
{
    private readonly IContractRepository _contractRepository;

    public GetContractByIdQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<GetContractsResponse> Handle(GetContractByIdQuery query, CancellationToken ct)
    {
        var id = ContractId.Of(query.ContractId);
        var entity = await _contractRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Contract with id '{query.ContractId}' not found.");

        return entity.Adapt<GetContractsResponse>();
    }
} 