using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate.Events;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Domain.CountryIdentityDocAggregate;

public class CountryIdentityDoc : Aggregate<CountryIdentityDocId>
{
    public CountryId CountryId { get; private set; }
    public IdentityDocumentId IdentityDocumentId { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private CountryIdentityDoc() { }

    public static CountryIdentityDoc Create(
        CountryIdentityDocId id,
        CountryId countryId,
        IdentityDocumentId identityDocumentId)
    {
        var countryIdentityDoc = new CountryIdentityDoc
        {
            Id = id,
            CountryId = countryId,
            IdentityDocumentId = identityDocumentId,
            IsEnabled = true
        };

        countryIdentityDoc.AddDomainEvent(new CountryIdentityDocCreatedEvent(
            countryIdentityDoc.Id.Value,
            countryIdentityDoc.CountryId.Value,
            countryIdentityDoc.IdentityDocumentId.Value,
            countryIdentityDoc.IsEnabled,
            DateTime.UtcNow));

        return countryIdentityDoc;
    }

    public void Update(
        CountryId countryId,
        IdentityDocumentId identityDocumentId,
        bool? isEnabled)
    {
        CountryId = countryId;
        IdentityDocumentId = identityDocumentId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CountryIdentityDocUpdatedEvent(
            Id.Value,
            CountryId.Value,
            IdentityDocumentId.Value,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        CountryId? countryId,
        IdentityDocumentId? identityDocumentId,
        bool? isEnabled)
    {
        CountryId = countryId ?? CountryId;
        IdentityDocumentId = identityDocumentId ?? IdentityDocumentId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CountryIdentityDocPatchedEvent(
            Id.Value,
            CountryId.Value,
            IdentityDocumentId.Value,
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