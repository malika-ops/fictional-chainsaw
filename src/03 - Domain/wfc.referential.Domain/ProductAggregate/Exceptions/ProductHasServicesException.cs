using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Domain.ProductAggregate.Exceptions;

public class ProductHasServicesException : BusinessException
{

    public ProductHasServicesException(IEnumerable<Service> services)
        : base($"Cannot delete the product because it has associated services.[{string.Join(", ", services.Select(c => $"{c.Name},{(c.Id is not null ? c.Id!.Value : null)}"))}]")
    {
    }

}
