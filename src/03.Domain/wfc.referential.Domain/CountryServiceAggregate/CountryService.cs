using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate.Events;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Domain.CountryServiceAggregate;

public class CountryService : Aggregate<CountryServiceId>
{
    public CountryId CountryId { get; private set; }
    public ServiceId ServiceId { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private CountryService() { }

    public static CountryService Create(
        CountryServiceId id,
        CountryId countryId,
        ServiceId serviceId)
    {
        var countryService = new CountryService
        {
            Id = id,
            CountryId = countryId,
            ServiceId = serviceId,
            IsEnabled = true
        };

        countryService.AddDomainEvent(new CountryIdentityDocCreatedEvent(
            countryService.Id.Value,
            countryService.CountryId.Value,
            countryService.ServiceId.Value,
            countryService.IsEnabled,
            DateTime.UtcNow));

        return countryService;
    }

    public void Update(
        CountryId countryId,
        ServiceId serviceId,
        bool? isEnabled)
    {
        CountryId = countryId;
        ServiceId = serviceId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CountryIdentityDocUpdatedEvent(
            Id.Value,
            CountryId.Value,
            ServiceId.Value,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        CountryId? countryId,
        ServiceId? serviceId,
        bool? isEnabled)
    {
        CountryId = countryId ?? CountryId;
        ServiceId = serviceId ?? ServiceId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CountryIdentityDocPatchedEvent(
            Id.Value,
            CountryId.Value,
            ServiceId.Value,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new CountryIdentityDocStatusChangedEvent(
            Id.Value,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new CountryIdentityDocStatusChangedEvent(
            Id.Value,
            IsEnabled,
            DateTime.UtcNow));
    }
}