namespace wfc.referential.Application.Services.Dtos;

public record GetFiltredServicesResponse
{
    public Guid ServiceId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string IsEnabled { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
}