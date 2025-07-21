using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.OperatorAggregate.Events;

namespace wfc.referential.Domain.OperatorAggregate;

public class Operator : Aggregate<OperatorId>
{
    public string Code { get; private set; } = string.Empty;
    public string IdentityCode { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;

    public OperatorType? OperatorType { get; private set; }

    public Guid? BranchId { get; private set; }

    public Guid? ProfileId { get; private set; }

    private Operator() { }

    public static Operator Create(
        OperatorId id,
        string code,
        string identityCode,
        string lastName,
        string firstName,
        string email,
        string phoneNumber,
        OperatorType? operatorType,
        Guid? branchId,
        Guid? profileId)
    {
        var operatorEntity = new Operator
        {
            Id = id,
            Code = code,
            IdentityCode = identityCode,
            LastName = lastName,
            FirstName = firstName,
            Email = email,
            PhoneNumber = phoneNumber,
            OperatorType = operatorType,
            BranchId = branchId,
            ProfileId = profileId
        };

        operatorEntity.AddDomainEvent(new OperatorCreatedEvent(
            operatorEntity.Id.Value,
            operatorEntity.Code,
            operatorEntity.IdentityCode,
            operatorEntity.LastName,
            operatorEntity.FirstName,
            operatorEntity.Email,
            operatorEntity.PhoneNumber,
            DateTime.UtcNow));

        return operatorEntity;
    }

    public void Update(
        string code,
        string identityCode,
        string lastName,
        string firstName,
        string email,
        string phoneNumber,
        bool? isEnabled,
        OperatorType? operatorType,
        Guid? branchId,
        Guid? profileId)
    {
        Code = code;
        IdentityCode = identityCode;
        LastName = lastName;
        FirstName = firstName;
        Email = email;
        PhoneNumber = phoneNumber;
        if (isEnabled.HasValue) IsEnabled = isEnabled.Value;

        OperatorType = operatorType;
        BranchId = branchId;
        ProfileId = profileId;

        AddDomainEvent(new OperatorUpdatedEvent(
            Id.Value,
            Code,
            IdentityCode,
            LastName,
            FirstName,
            Email,
            PhoneNumber,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        string? identityCode,
        string? lastName,
        string? firstName,
        string? email,
        string? phoneNumber,
        bool? isEnabled,
        OperatorType? operatorType,
        Guid? branchId,
        Guid? profileId)
    {
        Code = code ?? Code;
        IdentityCode = identityCode ?? IdentityCode;
        LastName = lastName ?? LastName;
        FirstName = firstName ?? FirstName;
        Email = email ?? Email;
        PhoneNumber = phoneNumber ?? PhoneNumber;
        if (isEnabled.HasValue) IsEnabled = isEnabled.Value;

        OperatorType = operatorType ?? OperatorType;
        BranchId = branchId ?? BranchId;
        ProfileId = profileId ?? ProfileId;

        AddDomainEvent(new OperatorPatchedEvent(
            Id.Value,
            Code,
            IdentityCode,
            LastName,
            FirstName,
            Email,
            PhoneNumber,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new OperatorDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new OperatorActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void SetOperatorType(OperatorType operatorType)
    {
        OperatorType = operatorType;
    }

    public void SetBranch(Guid branchId)
    {
        BranchId = branchId;
    }

    public void SetProfile(Guid profileId)
    {
        ProfileId = profileId;
    }
}