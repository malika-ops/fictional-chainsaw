using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Taxes.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Queries.GetTaxById;

public class GetTaxByIdQueryHandler : IQueryHandler<GetTaxByIdQuery, GetTaxesResponse>
{
    private readonly ITaxRepository _taxRepository;

    public GetTaxByIdQueryHandler(ITaxRepository taxRepository)
    {
        _taxRepository = taxRepository;
    }

    public async Task<GetTaxesResponse> Handle(GetTaxByIdQuery query, CancellationToken ct)
    {
        var id = TaxId.Of(query.TaxId);
        var entity = await _taxRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Tax with id '{query.TaxId}' not found.");

        return entity.Adapt<GetTaxesResponse>();
    }
} 