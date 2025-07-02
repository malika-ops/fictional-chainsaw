using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.ContractDetails.Dtos;

namespace wfc.referential.Application.ContractDetails.Queries.GetContractDetailById;

public record GetContractDetailByIdQuery : IQuery<GetContractDetailsResponse>
{
    public Guid ContractDetailsId { get; init; }
} 