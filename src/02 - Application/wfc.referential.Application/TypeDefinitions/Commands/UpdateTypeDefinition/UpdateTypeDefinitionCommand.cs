using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition
{
    public class UpdateTypeDefinitionCommand : ICommand<Guid>
    {
        public Guid TypeDefinitionId { get; set; }
        public string Libelle { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }


        public UpdateTypeDefinitionCommand(Guid typeDefinitionId, string libelle, string description, bool isEnabled)
        {
            TypeDefinitionId = typeDefinitionId;
            Libelle = libelle;
            Description = description;
            IsEnabled = isEnabled;
        }
    }
}