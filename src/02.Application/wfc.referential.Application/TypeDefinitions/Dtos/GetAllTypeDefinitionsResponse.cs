namespace wfc.referential.Application.TypeDefinitions.Dtos;

public record GetAllTypeDefinitionsResponse(
    Guid TypeDefinitionId,
    string Libelle,
    string Description,
    bool IsEnabled
);