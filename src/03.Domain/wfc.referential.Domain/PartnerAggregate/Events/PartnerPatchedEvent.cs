using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAggregate.Events;

public record PartnerPatchedEvent(
    Guid PartnerId,
    string Code,
    string Name,
    string PersonType,
    string ProfessionalTaxNumber,
    string WithholdingTaxRate,
    string HeadquartersCity,
    string HeadquartersAddress,
    string LastName,
    string FirstName,
    string PhoneNumberContact,
    string MailContact,
    string FunctionContact,
    string TransferType,
    string AuthenticationMode,
    string TaxIdentificationNumber,
    string TaxRegime,
    string AuxiliaryAccount,
    string ICE,
    string Logo,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;