using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Services.Commands.UpdateService;

public record UpdateServiceCommand : ICommand<Result<bool>>
{
    public Guid ServiceId { get; init; }
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsEnabled { get; init; } = true;
    public ProductId ProductId { get; init; } = default!;
}