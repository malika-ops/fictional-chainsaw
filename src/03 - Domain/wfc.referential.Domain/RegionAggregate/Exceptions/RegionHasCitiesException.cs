using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Domain.RegionAggregate.Exceptions;

public class RegionHasCitiesException : BusinessException
{

    public RegionHasCitiesException(IEnumerable<City> cityNames)
        : base($"Cannot delete the region because it has associated cities.[{string.Join(", ", cityNames.Select(c => $"{c.Name},{(c.Id is not null ? c.Id!.Value : null)}"))}]")
    {
    }

}
