using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Domain.TaxRuleDetailAggregate.Exceptions;

public class TaxRuleDetailAlreadyExistException : ConflictException
{
    public TaxRuleDetailAlreadyExistException(Guid taxRuleDetailId) : base($"{nameof(Tax)} ({taxRuleDetailId}) with the same CorridorId, TaxId, and ServiceId already exists.")
    {

    }
}
