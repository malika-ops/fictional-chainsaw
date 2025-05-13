using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;

public class CreateTypeDefinitionCommand : ICommand<Result<Guid>>
{
    public string Libelle { get; set; }
    public string Description { get; set; }

    public CreateTypeDefinitionCommand(string libelle, string description)
    {
        Libelle = libelle;
        Description = description;
    }
}