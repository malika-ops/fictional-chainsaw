using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Services.Commands.DeleteService;

public record DeleteServiceCommand : ICommand<Result<bool>>
{
    public Guid ServiceId { get; init; }
}