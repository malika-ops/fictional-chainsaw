using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAggregate.Events;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Domain.PartnerAggregate;

public class Partner : Aggregate<PartnerId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string PersonType { get; private set; } = string.Empty;
    public string ProfessionalTaxNumber { get; private set; } = string.Empty;
    public string WithholdingTaxRate { get; private set; } = string.Empty;
    public string HeadquartersCity { get; private set; } = string.Empty;
    public string HeadquartersAddress { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string PhoneNumberContact { get; private set; } = string.Empty;
    public string MailContact { get; private set; } = string.Empty;
    public string FunctionContact { get; private set; } = string.Empty;
    public string TransferType { get; private set; } = string.Empty;
    public string AuthenticationMode { get; private set; } = string.Empty;
    public string TaxIdentificationNumber { get; private set; } = string.Empty;
    public string TaxRegime { get; private set; } = string.Empty;
    public string AuxiliaryAccount { get; private set; } = string.Empty;
    public string ICE { get; private set; } = string.Empty;
    public string Logo { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public Guid? IdParent { get; private set; }

    // Relationships using ParamType instead of enums
    public ParamTypeId? NetworkModeId { get; private set; }
    public ParamType? NetworkMode { get; private set; }

    public ParamTypeId? PaymentModeId { get; private set; }
    public ParamType? PaymentMode { get; private set; }

    public ParamTypeId? PartnerTypeId { get; private set; }
    public ParamType? PartnerType { get; private set; }

    public ParamTypeId? SupportAccountTypeId { get; private set; }
    public ParamType? SupportAccountType { get; private set; }

    // Account relationships
    public Guid? CommissionAccountId { get; private set; }
    public PartnerAccount? CommissionAccount { get; private set; }

    public Guid? ActivityAccountId { get; private set; }
    public PartnerAccount? ActivityAccount { get; private set; }

    public Guid? SupportAccountId { get; private set; }
    public SupportAccount? SupportAccount { get; private set; }

    private Partner() { }

    public static Partner Create(
        PartnerId id,
        string code,
        string name,
        string personType,
        string professionalTaxNumber,
        string withholdingTaxRate,
        string headquartersCity,
        string headquartersAddress,
        string lastName,
        string firstName,
        string phoneNumberContact,
        string mailContact,
        string functionContact,
        string transferType,
        string authenticationMode,
        string taxIdentificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        string logo)
    {
        var partner = new Partner
        {
            Id = id,
            Code = code,
            Name = name,
            PersonType = personType,
            ProfessionalTaxNumber = professionalTaxNumber,
            WithholdingTaxRate = withholdingTaxRate,
            HeadquartersCity = headquartersCity,
            HeadquartersAddress = headquartersAddress,
            LastName = lastName,
            FirstName = firstName,
            PhoneNumberContact = phoneNumberContact,
            MailContact = mailContact,
            FunctionContact = functionContact,
            TransferType = transferType,
            AuthenticationMode = authenticationMode,
            TaxIdentificationNumber = taxIdentificationNumber,
            TaxRegime = taxRegime,
            AuxiliaryAccount = auxiliaryAccount,
            ICE = ice,
            Logo = logo,
            IsEnabled = true
        };

        partner.AddDomainEvent(new PartnerCreatedEvent(
            partner.Id.Value,
            partner.Code,
            partner.Name,
            partner.PersonType,
            partner.ProfessionalTaxNumber,
            partner.WithholdingTaxRate,
            partner.HeadquartersCity,
            partner.HeadquartersAddress,
            partner.LastName,
            partner.FirstName,
            partner.PhoneNumberContact,
            partner.MailContact,
            partner.FunctionContact,
            partner.TransferType,
            partner.AuthenticationMode,
            partner.TaxIdentificationNumber,
            partner.TaxRegime,
            partner.AuxiliaryAccount,
            partner.ICE,
            partner.Logo,
            partner.IsEnabled,
            DateTime.UtcNow));

        return partner;
    }

    public void Update(
        string code,
        string name,
        string personType,
        string professionalTaxNumber,
        string withholdingTaxRate,
        string headquartersCity,
        string headquartersAddress,
        string lastName,
        string firstName,
        string phoneNumberContact,
        string mailContact,
        string functionContact,
        string transferType,
        string authenticationMode,
        string taxIdentificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        string logo,
        bool? isEnabled)
    {
        Code = code;
        Name = name;
        PersonType = personType;
        ProfessionalTaxNumber = professionalTaxNumber;
        WithholdingTaxRate = withholdingTaxRate;
        HeadquartersCity = headquartersCity;
        HeadquartersAddress = headquartersAddress;
        LastName = lastName;
        FirstName = firstName;
        PhoneNumberContact = phoneNumberContact;
        MailContact = mailContact;
        FunctionContact = functionContact;
        TransferType = transferType;
        AuthenticationMode = authenticationMode;
        TaxIdentificationNumber = taxIdentificationNumber;
        TaxRegime = taxRegime;
        AuxiliaryAccount = auxiliaryAccount;
        ICE = ice;
        Logo = logo;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new PartnerUpdatedEvent(
            Id.Value,
            Code,
            Name,
            PersonType,
            ProfessionalTaxNumber,
            WithholdingTaxRate,
            HeadquartersCity,
            HeadquartersAddress,
            LastName,
            FirstName,
            PhoneNumberContact,
            MailContact,
            FunctionContact,
            TransferType,
            AuthenticationMode,
            TaxIdentificationNumber,
            TaxRegime,
            AuxiliaryAccount,
            ICE,
            Logo,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        string? name,
        string? personType,
        string? professionalTaxNumber,
        string? withholdingTaxRate,
        string? headquartersCity,
        string? headquartersAddress,
        string? lastName,
        string? firstName,
        string? phoneNumberContact,
        string? mailContact,
        string? functionContact,
        string? transferType,
        string? authenticationMode,
        string? taxIdentificationNumber,
        string? taxRegime,
        string? auxiliaryAccount,
        string? ice,
        string? logo,
        bool? isEnabled)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        PersonType = personType ?? PersonType;
        ProfessionalTaxNumber = professionalTaxNumber ?? ProfessionalTaxNumber;
        WithholdingTaxRate = withholdingTaxRate ?? WithholdingTaxRate;
        HeadquartersCity = headquartersCity ?? HeadquartersCity;
        HeadquartersAddress = headquartersAddress ?? HeadquartersAddress;
        LastName = lastName ?? LastName;
        FirstName = firstName ?? FirstName;
        PhoneNumberContact = phoneNumberContact ?? PhoneNumberContact;
        MailContact = mailContact ?? MailContact;
        FunctionContact = functionContact ?? FunctionContact;
        TransferType = transferType ?? TransferType;
        AuthenticationMode = authenticationMode ?? AuthenticationMode;
        TaxIdentificationNumber = taxIdentificationNumber ?? TaxIdentificationNumber;
        TaxRegime = taxRegime ?? TaxRegime;
        AuxiliaryAccount = auxiliaryAccount ?? AuxiliaryAccount;
        ICE = ice ?? ICE;
        Logo = logo ?? Logo;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new PartnerPatchedEvent(
            Id.Value,
            Code,
            Name,
            PersonType,
            ProfessionalTaxNumber,
            WithholdingTaxRate,
            HeadquartersCity,
            HeadquartersAddress,
            LastName,
            FirstName,
            PhoneNumberContact,
            MailContact,
            FunctionContact,
            TransferType,
            AuthenticationMode,
            TaxIdentificationNumber,
            TaxRegime,
            AuxiliaryAccount,
            ICE,
            Logo,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new PartnerDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new PartnerActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    // ParamType setters (similar to SupportAccount pattern)
    public void SetNetworkMode(ParamTypeId networkModeId)
    {
        NetworkModeId = networkModeId;
    }

    public void SetPaymentMode(ParamTypeId paymentModeId)
    {
        PaymentModeId = paymentModeId;
    }

    public void SetPartnerType(ParamTypeId partnerTypeId)
    {
        PartnerTypeId = partnerTypeId;
    }

    public void SetSupportAccountType(ParamTypeId supportAccountTypeId)
    {
        SupportAccountTypeId = supportAccountTypeId;
    }

    // Account relationship methods
    public void SetCommissionAccount(Guid accountId, PartnerAccount? account = null)
    {
        CommissionAccountId = accountId;
        if (account != null)
            CommissionAccount = account;
    }

    public void SetActivityAccount(Guid accountId, PartnerAccount? account = null)
    {
        ActivityAccountId = accountId;
        if (account != null)
            ActivityAccount = account;
    }

    public void SetSupportAccount(Guid accountId, SupportAccount? account = null)
    {
        SupportAccountId = accountId;
        if (account != null)
            SupportAccount = account;
    }

    public void SetParent(Guid? parentId)
    {
        IdParent = parentId;
    }
}