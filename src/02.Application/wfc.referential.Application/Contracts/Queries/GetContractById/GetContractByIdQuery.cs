using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Contracts.Dtos;

namespace wfc.referential.Application.Contracts.Queries.GetContractById;

public record GetContractByIdQuery : IQuery<GetContractsResponse>
{
    public Guid ContractId { get; init; }
} 