using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Commands.CreateOperator;

public record CreateOperatorCommand : ICommand<Result<Guid>>
{
    public string Code { get; init; } = string.Empty;
    public string IdentityCode { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public OperatorType? OperatorType { get; init; }
    public Guid? BranchId { get; init; }
}