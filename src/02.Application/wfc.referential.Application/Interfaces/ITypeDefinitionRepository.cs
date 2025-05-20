using wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ITypeDefinitionRepository
{
    Task<List<TypeDefinition>> GetAllTypeDefinitionsAsync(CancellationToken cancellationToken); 
    IQueryable<TypeDefinition> GetAllTypeDefinitionsQueryable(CancellationToken cancellationToken);
    Task<TypeDefinition?> GetByIdAsync(TypeDefinitionId id , CancellationToken cancellationToken);
    Task<TypeDefinition?> GetByLibelleAsync(string libelle, CancellationToken cancellationToken);
    Task<TypeDefinition> AddTypeDefinitionAsync(TypeDefinition typeDefinition, CancellationToken cancellationToken);
    Task UpdateTypeDefinitionAsync(TypeDefinition typeDefinition, CancellationToken cancellationToken);
    Task DeleteTypeDefinitionAsync(TypeDefinition typeDefinition, CancellationToken cancellationToken);

    Task<List<TypeDefinition>> GetFilteredTypeDefinitionsAsync(GetAllTypeDefinitionsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllTypeDefinitionsQuery request, CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}
