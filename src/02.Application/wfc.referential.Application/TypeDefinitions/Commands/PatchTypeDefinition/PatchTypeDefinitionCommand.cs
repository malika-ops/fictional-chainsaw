using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;

public record PatchTypeDefinitionCommand : ICommand<Result<bool>>
{
    public Guid TypeDefinitionId { get; init; }
    public string? Libelle { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }
}