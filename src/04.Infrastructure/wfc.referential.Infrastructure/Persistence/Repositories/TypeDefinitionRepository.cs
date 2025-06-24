using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class TypeDefinitionRepository : BaseRepository<TypeDefinition, TypeDefinitionId>, ITypeDefinitionRepository
{

    public TypeDefinitionRepository(ApplicationDbContext context) : base(context)
    {
    }
}