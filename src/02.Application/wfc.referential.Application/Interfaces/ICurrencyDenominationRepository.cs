using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICurrencyDenominationRepository : IRepositoryBase<CurrencyDenomination, CurrencyDenominationId>
{
}