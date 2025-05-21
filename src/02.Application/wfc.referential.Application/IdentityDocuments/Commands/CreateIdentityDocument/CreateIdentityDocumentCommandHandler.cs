using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate.Exceptions;

namespace wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;

public class CreateIdentityDocumentCommandHandler(IIdentityDocumentRepository repository,ICacheService cacheService) 
    : ICommandHandler<CreateIdentityDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
            throw new CodeAlreadyExistException(request.Code);

        var entity = IdentityDocument.Create(
            IdentityDocumentId.Of(Guid.NewGuid()),
            request.Code,
            request.Name,
            request.Description);

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveByPrefixAsync(CacheKeys.IdentityDocument.Prefix,cancellationToken);

        return Result.Success(entity.Id!.Value);
    }
}