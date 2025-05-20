using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Domain.CityAggregate.Exceptions;

public class CityHasCorridorException : BusinessException
{
    public CityHasCorridorException(Guid id):
        base($"{nameof(City)} with id {id} already has an assigned {nameof(Corridor)}.")
    {
        
    }
}
