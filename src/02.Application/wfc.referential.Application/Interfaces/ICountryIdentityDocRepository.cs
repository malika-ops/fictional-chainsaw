using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.CountryIdentityDocAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICountryIdentityDocRepository : IRepositoryBase<CountryIdentityDoc, CountryIdentityDocId>
{
}