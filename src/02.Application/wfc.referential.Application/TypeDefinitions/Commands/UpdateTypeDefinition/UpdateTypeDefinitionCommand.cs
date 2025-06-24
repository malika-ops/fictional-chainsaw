using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;

public record UpdateTypeDefinitionCommand : ICommand<Result<bool>>
{
    public Guid TypeDefinitionId { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}