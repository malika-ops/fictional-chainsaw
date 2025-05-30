using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate.Exceptions;

namespace wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;

public class CreateIdentityDocumentCommandHandler : ICommandHandler<CreateIdentityDocumentCommand, Result<Guid>>
{
    private readonly IIdentityDocumentRepository _identityDocumentRepository;

    public CreateIdentityDocumentCommandHandler(IIdentityDocumentRepository identityDocumentRepository)
        => _identityDocumentRepository = identityDocumentRepository;

    public async Task<Result<Guid>> Handle(CreateIdentityDocumentCommand command, CancellationToken ct)
    {
        var existingIdentityDocumentByCode = await _identityDocumentRepository.GetOneByConditionAsync(c => c.Code == command.Code, ct);
        if (existingIdentityDocumentByCode is not null)
            throw new IdentityDocumentCodeAlreadyExistException(command.Code);

        var identityDocument = IdentityDocument.Create(
            IdentityDocumentId.Of(Guid.NewGuid()),
            command.Code,
            command.Name,
            command.Description);

        await _identityDocumentRepository.AddAsync(identityDocument, ct);
        await _identityDocumentRepository.SaveChangesAsync(ct);

        return Result.Success(identityDocument.Id!.Value);
    }
}