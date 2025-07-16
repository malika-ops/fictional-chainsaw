using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Dtos;

public record CreateServiceRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public FlowDirection FlowDirection { get; init; } = FlowDirection.None;
    public Guid ProductId { get; init; }
}