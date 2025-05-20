using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyAggregate.Exception;

public class CodeAlreadyExistException : BusinessException
{
    public CodeAlreadyExistException(string code) : base($"The currency code '{code}' already exists.")
    {
    }
}
