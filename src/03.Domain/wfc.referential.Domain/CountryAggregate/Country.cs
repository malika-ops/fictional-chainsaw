using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries.Events;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Domain.Countries
{
    public class Country : Aggregate<CountryId>
    {
        public string Name { get; private set; }
        public string? Abbreviation { get; private set; }
        public string Code { get; private set; }
        public string ISO2 { get; private set; }
        public string ISO3 { get; private set; }
        public string DialingCode { get; private set; }
        public string TimeZone { get; private set; }
        public bool HasSector { get; private set; }
        public bool IsSmsEnabled { get; private set; }
        public int NumberDecimalDigits { get; private set; }
        public bool IsEnabled { get; private set; } = true;
        public Currency Currency { get; private set; }
        public CurrencyId CurrencyId { get; private set; }
        public List<Region> Regions { get; private set; } = [];
        public MonetaryZoneId MonetaryZoneId { get; private set; }


        private Country() { }

        public static Country Create(
           CountryId id,
           string? abbreviation,
           string name,
           string code,
           string ISO2,
           string ISO3,
           string dialingCode,
           string timeZone,
           bool hasSector,
           bool isSmsEnabled,
           int numberDecimalDigits,
           MonetaryZoneId monetaryZoneId,
           CurrencyId currencyId)
        {
            var country = new Country
            {
                Id = id,
                Abbreviation = abbreviation,
                Name = name,
                Code = code,
                ISO2 = ISO2,
                ISO3 = ISO3,
                DialingCode = dialingCode,
                TimeZone = timeZone,
                HasSector = hasSector,
                IsSmsEnabled = isSmsEnabled,
                NumberDecimalDigits = numberDecimalDigits,
                MonetaryZoneId = monetaryZoneId,
                CurrencyId = currencyId
            };

            // Raise the creation event for the country.
            country.AddDomainEvent(new CountryCreatedEvent(
                country.Id.Value,
                country.Code,
                country.Name,
                DateTime.UtcNow
            ));

            return country;
        }


        public void Update(
            string? abbreviation,
            string name,
            string code,
            string ISO2,
            string ISO3,
            string dialingCode,
            string timeZone,
            bool hasSector,
            bool isSmsEnabled,
            int numberDecimalDigits,
            MonetaryZoneId monetaryZoneId,
            CurrencyId currencyId,
            bool? isEnabled)
        {
            Abbreviation = abbreviation;
            Name = name;
            Code = code;
            this.ISO2 = ISO2;
            this.ISO3 = ISO3;
            DialingCode = dialingCode;
            TimeZone = timeZone;
            HasSector = hasSector;
            IsSmsEnabled = isSmsEnabled;
            NumberDecimalDigits = numberDecimalDigits;
            MonetaryZoneId = monetaryZoneId;
            CurrencyId = currencyId;
            IsEnabled = isEnabled ?? IsEnabled;

            // Raise the update event for the country.
            AddDomainEvent(new CountryUpdatedEvent(
                Id!.Value,
                Code,
                Name,
                DateTime.UtcNow
            ));
        }

        public void Patch(
            string? abbreviation,
            string? name,
            string? code,
            string? ISO2,
            string? ISO3,
            string? dialingCode,
            string? timeZone,
            bool? hasSector,
            bool? isSmsEnabled,
            int? numberDecimalDigits,
            MonetaryZoneId? monetaryZoneId,
            CurrencyId? currencyId,
            bool? isEnabled)
        {
            Abbreviation = abbreviation ?? Abbreviation;
            Name = name ?? Name;
            Code = code ?? Code;
            this.ISO2 = ISO2 ?? this.ISO2;
            this.ISO3 = ISO3 ?? this.ISO3;
            DialingCode = dialingCode ?? DialingCode;
            TimeZone = timeZone ?? TimeZone;
            HasSector = hasSector ?? HasSector;
            IsSmsEnabled = isSmsEnabled ?? IsSmsEnabled;
            NumberDecimalDigits = numberDecimalDigits ?? NumberDecimalDigits;
            MonetaryZoneId = monetaryZoneId ?? MonetaryZoneId;
            CurrencyId = currencyId ?? CurrencyId;
            IsEnabled = isEnabled ?? IsEnabled;

            // Raise the patch event for the country.
            AddDomainEvent(new CountryPatchedEvent(
                Id!.Value,
                Abbreviation,
                Name,
                Code,
                this.ISO2,
                this.ISO3,
                DialingCode,
                TimeZone,
                IsEnabled,
                MonetaryZoneId.Value,
                CurrencyId.Value,
                DateTime.UtcNow
            ));
        }
     

        public void Disable()
        {
            IsEnabled = false;
            // Raise the disabled event.
            AddDomainEvent(new CountryDisabledEvent(
                Id!.Value,
                DateTime.UtcNow
            ));
        }
    }

}
