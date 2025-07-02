using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Application.Currencies.Queries.GetCurrencyById;

public class GetCurrencyByIdQueryHandler : IQueryHandler<GetCurrencyByIdQuery, GetCurrenciesResponse>
{
    private readonly ICurrencyRepository _currencyRepository;

    public GetCurrencyByIdQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<GetCurrenciesResponse> Handle(GetCurrencyByIdQuery query, CancellationToken ct)
    {
        var id = CurrencyId.Of(query.CurrencyId);
        var entity = await _currencyRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Currency with id '{query.CurrencyId}' not found.");

        return entity.Adapt<GetCurrenciesResponse>();
    }
} 