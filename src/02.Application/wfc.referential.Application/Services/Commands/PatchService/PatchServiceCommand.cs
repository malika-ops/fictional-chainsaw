using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Services.Commands.PatchService;

public record PatchServiceCommand : ICommand<Result<bool>>
{
    public Guid ServiceId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
    public Guid? ProductId { get; init; }
}