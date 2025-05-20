namespace wfc.referential.Application.Services.Dtos;

public record DeleteServiceRequest
{
    public Guid ServiceId { get; init; }
}
