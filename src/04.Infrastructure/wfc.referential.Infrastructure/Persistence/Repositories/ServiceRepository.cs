using System.Linq.Expressions;
using BuildingBlocks.Core.Abstraction.Repositories;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Services.Queries.GetFiltredServices;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class ServiceRepository : BaseRepository<Service, ServiceId> , IServiceRepository
{
    public ServiceRepository(ApplicationDbContext context) : base(context)
    {
    }
}