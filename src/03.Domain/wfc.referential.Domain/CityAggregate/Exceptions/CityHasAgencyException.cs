using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Domain.CityAggregate.Exceptions;

public class CityHasAgencyException : BusinessException
{
    public CityHasAgencyException(Guid id):
        base($"{nameof(City)} with Id {id} already has an assigned {nameof(Agency)}.")
    {
        
    }
}
