using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICurrencyRepository : IRepositoryBase<Currency, CurrencyId>
{
}