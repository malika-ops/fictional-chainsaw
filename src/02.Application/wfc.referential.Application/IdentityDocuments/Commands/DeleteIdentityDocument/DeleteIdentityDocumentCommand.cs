using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;

public record DeleteIdentityDocumentCommand : ICommand<Result<bool>>
{
    public Guid IdentityDocumentId { get; }
    public DeleteIdentityDocumentCommand(Guid identityDocumentId) => IdentityDocumentId = identityDocumentId;
}