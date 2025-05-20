using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;

public class DeleteCountryIdentityDocCommandHandler : ICommandHandler<DeleteCountryIdentityDocCommand, bool>
{
    private readonly ICountryIdentityDocRepository _repository;

    public DeleteCountryIdentityDocCommandHandler(ICountryIdentityDocRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCountryIdentityDocCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.CountryIdentityDocId, cancellationToken);
        if (entity == null)
            throw new CountryIdentityDocException("CountryIdentityDoc not found");

        entity.Disable();

        await _repository.UpdateAsync(entity, cancellationToken);

        return true;
    }
}