using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;

public record DeleteIdentityDocumentCommand (Guid IdentityDocumentId)
    : ICommand<Result<bool>>;