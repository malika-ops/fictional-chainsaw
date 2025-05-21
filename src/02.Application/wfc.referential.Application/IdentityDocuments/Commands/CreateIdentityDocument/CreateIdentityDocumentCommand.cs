using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;

public record CreateIdentityDocumentCommand(string Code,string Name, string Description) 
    : ICommand<Result<Guid>>;