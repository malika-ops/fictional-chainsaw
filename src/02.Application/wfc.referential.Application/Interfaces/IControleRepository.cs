using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.ControleAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IControleRepository : IRepositoryBase<Controle, ControleId>
{
}