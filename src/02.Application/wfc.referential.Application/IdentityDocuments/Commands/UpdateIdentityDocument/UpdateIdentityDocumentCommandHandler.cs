using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate.Exceptions;

namespace wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;

public class UpdateIdentityDocumentCommandHandler
    : ICommandHandler<UpdateIdentityDocumentCommand, Result<bool>>
{
    private readonly IIdentityDocumentRepository _repo;

    public UpdateIdentityDocumentCommandHandler(IIdentityDocumentRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(UpdateIdentityDocumentCommand cmd, CancellationToken ct)
    {
        var identityDocument = await _repo.GetByIdAsync(IdentityDocumentId.Of(cmd.IdentityDocumentId), ct);
        if (identityDocument is null)
            throw new ResourceNotFoundException($"Identity document [{cmd.IdentityDocumentId}] not found.");

        // uniqueness on Code
        var duplicateCode = await _repo.GetOneByConditionAsync(c => c.Code == cmd.Code, ct);
        if (duplicateCode is not null && duplicateCode.Id != identityDocument.Id)
            throw new IdentityDocumentCodeAlreadyExistException(cmd.Code);

        identityDocument.Update(
            cmd.Code,
            cmd.Name,
            cmd.Description,
            cmd.IsEnabled);

        _repo.Update(identityDocument);
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}