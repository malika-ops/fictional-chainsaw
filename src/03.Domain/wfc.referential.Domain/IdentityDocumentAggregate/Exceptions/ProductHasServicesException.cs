using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Domain.IdentityDocumentAggregate.Exceptions;

public class ServiceHasPricingssException : BusinessException
{

    public ServiceHasPricingssException(IEnumerable<Service> cityNames)
        : base($"Cannot delete the product because it has associated cities.[{string.Join(", ", cityNames.Select(c => $"{c.Name},{(c.Id is not null ? c.Id!.Value : null)}"))}]")
    {
    }

}
