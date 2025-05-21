using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Domain.TaxAggregate.Exceptions;

public class TaxHasTaxRuleDetailsException : BusinessException
{
    public TaxHasTaxRuleDetailsException(Guid id):
        base($"{nameof(Tax)} with id {id} already has an assigned {nameof(TaxRuleDetail)}.")
    {
        
    }
}
