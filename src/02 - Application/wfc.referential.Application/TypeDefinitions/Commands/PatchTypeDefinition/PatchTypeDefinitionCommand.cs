using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;

public class PatchTypeDefinitionCommand : ICommand<Result<Guid>>
{
    // The ID from the route
    public Guid TypeDefinitionId { get; }

    // The optional fields to update
    public string? Libelle { get; }
    public string? Description { get; }
    public bool? IsEnabled { get; }

    public PatchTypeDefinitionCommand(
        Guid typeDefinitionId,
        string? libelle,
        string? description,
        bool? isEnabled)
    {
        TypeDefinitionId = typeDefinitionId;
        Libelle = libelle;
        Description = description;
        IsEnabled = isEnabled;
    }
}