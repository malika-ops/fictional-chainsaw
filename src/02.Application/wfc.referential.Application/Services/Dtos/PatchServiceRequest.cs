using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Dtos;

public record PatchServiceRequest
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public FlowDirection? FlowDirection { get; init; }
    public bool? IsEnabled { get; init; }
}

