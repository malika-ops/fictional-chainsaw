using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ITaxRepository : IRepositoryBase<Tax, TaxId>
{
}
