using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.BankAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IBankRepository : IRepositoryBase<Bank, BankId>
{
}