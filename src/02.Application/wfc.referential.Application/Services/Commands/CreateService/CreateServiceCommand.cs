using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Services.Commands.CreateService;

public record CreateServiceCommand : ICommand<Result<Guid>>
{
    public Guid ServiceId { get; init; } = default!;
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsEnabled { get; init; } = true;
    public Guid ProductId { get; init; } = default!;

}