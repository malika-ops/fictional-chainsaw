using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.ContractDetails.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractDetailsAggregate;

namespace wfc.referential.Application.ContractDetails.Queries.GetContractDetailById;

public class GetContractDetailByIdQueryHandler : IQueryHandler<GetContractDetailByIdQuery, GetContractDetailsResponse>
{
    private readonly IContractDetailsRepository _contractDetailsRepository;

    public GetContractDetailByIdQueryHandler(IContractDetailsRepository contractDetailsRepository)
    {
        _contractDetailsRepository = contractDetailsRepository;
    }

    public async Task<GetContractDetailsResponse> Handle(GetContractDetailByIdQuery query, CancellationToken ct)
    {
        var id = ContractDetailsId.Of(query.ContractDetailsId);
        var entity = await _contractDetailsRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"ContractDetail with id '{query.ContractDetailsId}' not found.");

        return entity.Adapt<GetContractDetailsResponse>();
    }
} 