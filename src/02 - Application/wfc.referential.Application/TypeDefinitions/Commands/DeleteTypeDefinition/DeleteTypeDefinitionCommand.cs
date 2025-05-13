using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition
{
    public class DeleteTypeDefinitionCommand : ICommand<bool>
    {
        public Guid TypeDefinitionId { get; set; }

        public DeleteTypeDefinitionCommand(Guid typeDefinitionId)
        {
            TypeDefinitionId = typeDefinitionId;
        }
    }
}
