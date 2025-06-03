using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CityAggregate.Exceptions;

public class CityNotFoundException : ResourceNotFoundException
{
    public CityNotFoundException(Guid id): base($"{nameof(City)} with id : {id} not found")
    {
        
    }
}
