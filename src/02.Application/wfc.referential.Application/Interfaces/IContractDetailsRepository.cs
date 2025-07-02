using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.ContractDetailsAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IContractDetailsRepository : IRepositoryBase<Domain.ContractDetailsAggregate.ContractDetails, ContractDetailsId>
{
}