namespace wfc.referential.Application.Services.Dtos;

public record PatchServiceRequest
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
}

