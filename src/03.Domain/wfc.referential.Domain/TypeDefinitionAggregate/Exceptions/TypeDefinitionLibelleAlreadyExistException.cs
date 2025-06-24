using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

public class TypeDefinitionLibelleAlreadyExistException(string libelle)
    : ConflictException($"TypeDefinition with libelle '{libelle}' already exists.");