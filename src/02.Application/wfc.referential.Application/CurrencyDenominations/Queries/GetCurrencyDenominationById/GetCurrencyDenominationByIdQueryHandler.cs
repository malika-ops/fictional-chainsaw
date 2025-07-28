using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.CurrencyDenominations.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Queries.GetCurrencyDenominationById;

public class GetCurrencyDenominationByIdQueryHandler : IQueryHandler<GetCurrencyDenominationByIdQuery, GetCurrencyDenominationsResponse>
{
    private readonly ICurrencyDenominationRepository _CurrencyDenominationRepository;

    public GetCurrencyDenominationByIdQueryHandler(ICurrencyDenominationRepository CurrencyDenominationRepository)
    {
        _CurrencyDenominationRepository = CurrencyDenominationRepository;
    }

    public async Task<GetCurrencyDenominationsResponse> Handle(GetCurrencyDenominationByIdQuery query, CancellationToken ct)
    {
        var id = CurrencyDenominationId.Of(query.CurrencyDenominationId);
        var entity = await _CurrencyDenominationRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"CurrencyDenomination with id '{query.CurrencyDenominationId}' not found.");

        return entity.Adapt<GetCurrencyDenominationsResponse>();
    }
} 