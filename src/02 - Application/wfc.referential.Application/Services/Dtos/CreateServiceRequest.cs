namespace wfc.referential.Application.Services.Dtos;

public record CreateServiceRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
}