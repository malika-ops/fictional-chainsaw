namespace wfc.referential.Application.Tiers.Dtos;

public record TierResponse
{
    public Guid TierId {  get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}
    
