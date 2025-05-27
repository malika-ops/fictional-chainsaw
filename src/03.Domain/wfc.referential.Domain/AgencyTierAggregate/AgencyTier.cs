using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate.Events;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Domain.AgencyTierAggregate;

public class AgencyTier : Aggregate<AgencyTierId>
{
    public AgencyId AgencyId { get; private set; }
    public TierId TierId { get; private set; }

    public string Code { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;

    public Agency? Agency { get; private set; }
    public Tier? Tier { get; private set; }

    private AgencyTier() { }

    public static AgencyTier Create(
       AgencyTierId id,
       AgencyId agencyId,
       TierId tierId,
       string code,
       string? password)
    {

        var entity = new AgencyTier
        {
            Id = id,
            AgencyId = agencyId,
            TierId = tierId,
            Code = code,
            Password = password ?? string.Empty,
        };

        entity.AddDomainEvent(new AgencyTierCreatedEvent(
            entity.Id.Value,
            entity.AgencyId.Value,
            entity.TierId.Value,
            entity.Code,
            DateTime.UtcNow));

        return entity;

    }

    public void Update(
    AgencyId agencyId,
    TierId tierId,
    string code,
    string password,
    bool isEnabled)
    {
        AgencyId = agencyId;
        TierId = tierId;
        Code = code;
        Password = password;
        IsEnabled = isEnabled;

        AddDomainEvent(new AgencyTierUpdatedEvent(
            Id.Value,
            AgencyId.Value,
            TierId.Value,
            Code,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
    AgencyId agencyId,
    TierId tierId,
    string? code,
    string? password,
    bool? isEnabled)
    {
        AgencyId = agencyId;
        TierId = tierId;
        Code = code ?? Code;
        Password = password ?? Password;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new AgencyTierPatchedEvent(
           Id!.Value,
           AgencyId.Value,
           TierId.Value,
           Code,
           IsEnabled,
           DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;
        AddDomainEvent(new AgencyTierDisabledEvent(Id.Value, DateTime.UtcNow));
    }

}