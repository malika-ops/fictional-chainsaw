using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;

public class CreateTypeDefinitionCommandHandler : ICommandHandler<CreateTypeDefinitionCommand, Result<Guid>>
{
    private readonly ITypeDefinitionRepository _typeDefinitionRepository;

    public CreateTypeDefinitionCommandHandler(ITypeDefinitionRepository typeDefinitionRepository)
    {
        _typeDefinitionRepository = typeDefinitionRepository;
    }

    public async Task<Result<Guid>> Handle(CreateTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var id = TypeDefinitionId.Of(Guid.NewGuid());
        var typedefinition = TypeDefinition.Create(id, request.Libelle, request.Description, []);

        await _typeDefinitionRepository.AddTypeDefinitionAsync(typedefinition, cancellationToken);

        return Result.Success(typedefinition.Id.Value);
    }
}