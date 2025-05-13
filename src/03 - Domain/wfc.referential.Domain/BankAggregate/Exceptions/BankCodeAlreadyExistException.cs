using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.BankAggregate.Exceptions;

public class BankCodeAlreadyExistException : BusinessException
{
    public BankCodeAlreadyExistException(string code) : base($"Bank with code {code} already exists.")
    {
    }
}