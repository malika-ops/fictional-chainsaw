using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;

public record UpdateIdentityDocumentCommand(Guid IdentityDocumentId, string Code, string Name, string Description, bool IsEnabled) 
    : ICommand<Result<Guid>>;