namespace wfc.referential.Application.TypeDefinitions.Dtos;

public record GetFiltredTypeDefinitionsResponse(
    Guid TypeDefinitionId,
    string Libelle,
    string Description,
    bool IsEnabled
);