using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.Application.Services.Queries.GetServiceById;

public record GetServiceByIdQuery : IQuery<GetServicesResponse>
{
    public Guid ServiceId { get; init; }
} 