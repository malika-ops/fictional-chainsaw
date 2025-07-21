using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Commands.UpdateOperator;

public record UpdateOperatorCommand : ICommand<Result<bool>>
{
    public Guid OperatorId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string IdentityCode { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public OperatorType? OperatorType { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ProfileId { get; init; }
}