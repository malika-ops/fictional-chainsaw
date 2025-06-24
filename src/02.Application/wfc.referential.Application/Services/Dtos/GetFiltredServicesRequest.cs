namespace wfc.referential.Application.Services.Dtos;

public record GetFiltredServicesRequest : FilterRequest
{
    public string? Code { get; init; }
    public string? Name { get; init; }
}