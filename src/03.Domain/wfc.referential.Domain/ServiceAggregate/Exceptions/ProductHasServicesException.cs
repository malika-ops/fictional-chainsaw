using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ServiceAggregate.Exceptions;

public class ServiceHasPricingssException : BusinessException
{

    public ServiceHasPricingssException(IEnumerable<Service> cityNames)
        : base($"Cannot delete the product because it has associated cities.[{string.Join(", ", cityNames.Select(c => $"{c.Name},{(c.Id is not null ? c.Id!.Value : null)}"))}]")
    {
    }

}
