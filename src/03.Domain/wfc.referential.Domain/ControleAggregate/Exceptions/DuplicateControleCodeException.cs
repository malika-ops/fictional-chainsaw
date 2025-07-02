using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ControleAggregate.Exceptions;

public class DuplicateControleCodeException : ConflictException
{
    public DuplicateControleCodeException(string code)
        : base($"Controle with code '{code}' already exists.") { }
}