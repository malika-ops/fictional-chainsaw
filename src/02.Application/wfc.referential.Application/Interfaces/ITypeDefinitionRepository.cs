using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ITypeDefinitionRepository : IRepositoryBase<TypeDefinition,TypeDefinitionId>
{

}
