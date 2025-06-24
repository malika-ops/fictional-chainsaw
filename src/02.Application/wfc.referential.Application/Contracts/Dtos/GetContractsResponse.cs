namespace wfc.referential.Application.Contracts.Dtos;

public record GetContractsResponse
{
    public Guid ContractId { get; init; }
    public string Code { get; init; } = string.Empty;
    public Guid PartnerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsEnabled { get; init; }
}
