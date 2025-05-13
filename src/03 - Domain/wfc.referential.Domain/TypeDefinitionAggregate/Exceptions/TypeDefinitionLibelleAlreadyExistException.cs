using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

public class TypeDefinitionLibelleAlreadyExistException : BusinessException
{
    public TypeDefinitionLibelleAlreadyExistException(string libelle)
        : base($"TypeDefinition with libelle {libelle} already exists.")
    {
    }
}