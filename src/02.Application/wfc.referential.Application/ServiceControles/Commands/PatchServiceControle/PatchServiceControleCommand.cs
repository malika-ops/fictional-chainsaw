using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ServiceControles.Commands.PatchServiceControle;

public record PatchServiceControleCommand : ICommand<Result<bool>>
{
    public Guid ServiceControleId { get; init; }
    public Guid? ServiceId { get; init; }
    public Guid? ControleId { get; init; }
    public Guid? ChannelId { get; init; }
    public int? ExecOrder { get; init; }
    public bool? IsEnabled { get; init; }
}