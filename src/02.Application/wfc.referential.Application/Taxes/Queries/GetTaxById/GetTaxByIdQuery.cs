using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Taxes.Dtos;

namespace wfc.referential.Application.Taxes.Queries.GetTaxById;

public record GetTaxByIdQuery : IQuery<GetTaxesResponse>
{
    public Guid TaxId { get; init; }
} 