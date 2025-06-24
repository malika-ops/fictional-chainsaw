using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.ContractAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IContractRepository : IRepositoryBase<Contract, ContractId>
{
}