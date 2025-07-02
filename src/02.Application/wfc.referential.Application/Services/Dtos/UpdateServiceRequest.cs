namespace wfc.referential.Application.Services.Dtos;

public record UpdateServiceRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public Guid ProductId { get; init; }
}
