using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IOperatorRepository : IRepositoryBase<Operator, OperatorId>
{
}