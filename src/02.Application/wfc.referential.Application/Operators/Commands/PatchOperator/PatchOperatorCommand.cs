using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Commands.PatchOperator;

public record PatchOperatorCommand : ICommand<Result<bool>>
{
    public Guid OperatorId { get; init; }
    public string? Code { get; init; }
    public string? IdentityCode { get; init; }
    public string? LastName { get; init; }
    public string? FirstName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool? IsEnabled { get; init; }
    public OperatorType? OperatorType { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ProfileId { get; init; }
}