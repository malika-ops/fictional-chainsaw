using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyAggregate.Exception;

public class CodeIsoAlreadyExistException : BusinessException
{
    public CodeIsoAlreadyExistException(int codeiso) : base($"The currency number '{codeiso}' already exists.")
    {
    }
}