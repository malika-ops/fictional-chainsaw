using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Domain.CityAggregate.Exceptions;

public class CityHasSectorException : BusinessException
{
    public CityHasSectorException(Guid id) : 
        base($"{nameof(City)} with code {id} already has an assigned {nameof(Sector)}.")
    {
        
    }
}
