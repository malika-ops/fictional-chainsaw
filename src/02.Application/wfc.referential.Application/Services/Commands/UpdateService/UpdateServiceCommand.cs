using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Commands.UpdateService;

public record UpdateServiceCommand : ICommand<Result<bool>>
{
    public Guid ServiceId { get; init; }
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public FlowDirection FlowDirection { get; init; } = FlowDirection.None;
    public bool IsEnabled { get; init; } = true;
    public ProductId ProductId { get; init; } = default!;
}