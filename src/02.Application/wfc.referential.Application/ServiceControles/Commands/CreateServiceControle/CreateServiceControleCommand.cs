using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ServiceControles.Commands.CreateServiceControle;

public record CreateServiceControleCommand : ICommand<Result<Guid>>
{
    public Guid ServiceId { get; init; }
    public Guid ControleId { get; init; }
    public Guid ChannelId { get; init; }
    public int ExecOrder { get; init; }
}