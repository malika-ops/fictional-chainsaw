using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Domain.ServiceAggregate.Exceptions;

public class ServiceHasTaxesException : BusinessException
{

    public ServiceHasTaxesException(IEnumerable<TaxRuleDetail> taxRuleDetails)
        : base($"Cannot delete the service because it has associated taxes.[{string.Join(", ", taxRuleDetails.Select(c => $"{c.TaxId},{(c.Id is not null ? c.Id!.Value : null)}"))}]")
    {
    }

}
