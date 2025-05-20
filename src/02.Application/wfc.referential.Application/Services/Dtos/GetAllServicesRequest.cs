namespace wfc.referential.Application.Services.Dtos;

public record GetAllServicesRequest
{
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; } = true;
}