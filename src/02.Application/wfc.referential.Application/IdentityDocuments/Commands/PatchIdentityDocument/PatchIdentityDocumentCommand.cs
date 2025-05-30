using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;

public record PatchIdentityDocumentCommand : ICommand<Result<bool>>
{
    public Guid IdentityDocumentId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }
}