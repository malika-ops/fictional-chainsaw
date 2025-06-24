using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Affiliates.Commands.DeleteAffiliate;

public record DeleteAffiliateCommand : ICommand<Result<bool>>
{
    public Guid AffiliateId { get; init; }
}