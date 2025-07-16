using BuildingBlocks.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Infrastructure.Data;
public static class DbInitializer
{
    public static async Task SeedWithCache(ApplicationDbContext context, ICacheService cacheService)
    {

        if (!context.Countries.Any())
        {
            var monetaryzoneMA = MonetaryZone.Create(new MonetaryZoneId(Guid.NewGuid()), "MAR", "Maroc", "MA");
            var monetaryzoneEU = MonetaryZone.Create(new MonetaryZoneId(Guid.NewGuid()), "EU", "Europe", "EU");

            var currencyMAD = Currency.Create(new CurrencyId(Guid.NewGuid()), "MAD", "Dirham marocain", "MAD", "MAD", 2);
            context.Currencies.Add(currencyMAD);

            context.MonetaryZones.Add(monetaryzoneMA);
            context.MonetaryZones.Add(monetaryzoneEU);
            context.SaveChanges();


            // ✅ 1️⃣ Ajouter les countries
            var maroc = Country.Create(new CountryId(Guid.NewGuid()), "Maroc", "Maroc", "MAR", "MA", "MAR", "+212", "0", true, true, 2, monetaryzoneMA.Id!, currencyMAD.Id!);
            var egypte = Country.Create(new CountryId(Guid.NewGuid()), "Egypte", "Egypte", "EGY", "EG", "EGY", "+20", "+2", false, false, 2, monetaryzoneMA.Id!, currencyMAD.Id!);
            var senegal = Country.Create(new CountryId(Guid.NewGuid()), "Senegal", "Senegal", "SEN", "SE", "SEN", "+221", "0", false, false, 2, monetaryzoneMA.Id!, currencyMAD.Id!);
            var france = Country.Create(new CountryId(Guid.NewGuid()), "France", "France", "FRA", "FR", "FRA", "+33", "+1", false, false, 2, monetaryzoneEU.Id!, currencyMAD.Id!);
            var suisse = Country.Create(new CountryId(Guid.NewGuid()), "Suisse", "Suisse", "CHE", "CH", "CHE", "+41", "+1", false, false, 2, monetaryzoneEU.Id!, currencyMAD.Id!);


            context.Countries.Add(maroc);
            context.Countries.Add(egypte);
            context.Countries.Add(senegal);
            context.Countries.Add(france);
            context.Countries.Add(suisse);

            context.SaveChanges();

            // ✅ 2️⃣ Ajouter les régions officielles du Maroc
            var regions = new List<Region>
            {
                Region.Create(new RegionId(Guid.NewGuid()), "10", "Tanger-Tétouan-Al Hoceïma", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "20", "L'Oriental", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "30", "Fès-Meknès", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "40", "Rabat-Salé-Kénitra", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "50", "Béni Mellal-Khénifra", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "60", "Casablanca-Settat", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "70", "Marrakech-Safi", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "80", "Drâa-Tafilalet", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "90", "Souss-Massa", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "100", "Guelmim-Oued Noun", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "110", "Laâyoune-Sakia El Hamra", maroc.Id),
                Region.Create(new RegionId(Guid.NewGuid()), "120", "Dakhla-Oued Ed-Dahab", maroc.Id),
            };

            context.Regions.AddRange(regions);
            context.SaveChanges();

            // ✅ 3️⃣ Ajouter les villes principales de chaque région
            var villes = new List<City>();

            foreach (var region in regions)
            {
                switch (region.Name)
                {
                    case "Tanger-Tétouan-Al Hoceïma":
                        villes.AddRange(new[]
                        {
                            City.Create(CityId.Of(Guid.NewGuid()), "code", "name", "timezone",  region.Id, "abbreviation"),
                            City.Create(new CityId(Guid.NewGuid()), "110101", "Tétouan", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "110102", "Al Hoceïma", "timezone",   region.Id, null),
                        });
                        break;

                    case "L'Oriental":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "120100", "Oujda", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "120101", "Nador", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "120102", "Berkane", "timezone",   region.Id, null),
                        });
                        break;

                    case "Fès-Meknès":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "130100", "Fès", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "130101", "Meknès", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "130102", "Ifrane", "timezone",   region.Id, null),
                        });
                        break;

                    case "Rabat-Salé-Kénitra":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "140100", "Rabat", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "140101", "Salé", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "140102", "Kénitra", "timezone",   region.Id, null),
                        });
                        break;

                    case "Béni Mellal-Khénifra":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "150100", "Béni Mellal", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "150101", "Khénifra", "timezone",   region.Id, null),
                        });
                        break;

                    case "Casablanca-Settat":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "160100", "Casablanca", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "160101", "Mohammedia", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "160102", "El Jadida", "timezone",   region.Id, null),
                        });
                        break;

                    case "Marrakech-Safi":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "170100", "Marrakech", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "170101", "Safi", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "170102", "Essaouira", "timezone",   region.Id, null),
                        });
                        break;

                    case "Drâa-Tafilalet":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "180100", "Errachidia", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "180101", "Ouarzazate", "timezone",   region.Id, null),
                        });
                        break;

                    case "Souss-Massa":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "190100", "Agadir", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "190101", "Taroudant", "timezone",   region.Id, null),
                        });
                        break;

                    case "Guelmim-Oued Noun":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "1100100", "Guelmim", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "1100101", "Tan-Tan", "timezone",   region.Id, null),
                        });
                        break;

                    case "Laâyoune-Sakia El Hamra":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "1200100", "Laâyoune", "timezone",   region.Id, null),
                            City.Create(new CityId(Guid.NewGuid()), "1200101", "Boujdour", "timezone",   region.Id, null),
                        });
                        break;

                    case "Dakhla-Oued Ed-Dahab":
                        villes.AddRange(new[]
                        {
                            City.Create(new CityId(Guid.NewGuid()), "1300100", "Dakhla", "timezone",   region.Id, null),
                        });
                        break;
                }

            }

            //Insérer toutes les villes
            context.Cities.AddRange(villes);
            context.SaveChanges();

            // ✅ Add banks
            var banks = new List<Bank>
            {
                Bank.Create(new BankId(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB"),
                Bank.Create(new BankId(Guid.NewGuid()), "BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE"),
                Bank.Create(new BankId(Guid.NewGuid()), "SG", "Société Générale Maroc", "SG"),
                Bank.Create(new BankId(Guid.NewGuid()), "BP", "Banque Populaire", "BP"),
                Bank.Create(new BankId(Guid.NewGuid()), "BMCI", "Banque Marocaine pour le Commerce et l'Industrie", "BMCI"),
                Bank.Create(new BankId(Guid.NewGuid()), "CAM", "Crédit Agricole du Maroc", "CAM"),
                Bank.Create(new BankId(Guid.NewGuid()), "CDM", "Crédit du Maroc", "CDM"),
                Bank.Create(new BankId(Guid.NewGuid()), "CIH", "Crédit Immobilier et Hôtelier", "CIH")
            };

            context.Banks.AddRange(banks);
            context.SaveChanges();

            // Create account type definition
            var accountTypeDef = TypeDefinition.Create(
                TypeDefinitionId.Of(Guid.NewGuid()),
                libelle: "AccountType",
                description: "Type of partner account"
            );

            context.TypeDefinitions.Add(accountTypeDef);
            context.SaveChanges();

            // Create param types for account types
            var activityParamType = ParamType.Create(
                ParamTypeId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                accountTypeDef.Id,
                "Activity"
            );

            var commissionParamType = ParamType.Create(
                ParamTypeId.Of(Guid.Parse("33333333-3333-3333-3333-333333333333")),
                accountTypeDef.Id,
                "Commission"
            );

            accountTypeDef.AddParamType(activityParamType);
            accountTypeDef.AddParamType(commissionParamType);

            context.ParamTypes.Add(activityParamType);
            context.ParamTypes.Add(commissionParamType);
            context.SaveChanges();

            // ✅ Add partner accounts for Casablanca
            var casablancaCity = villes.FirstOrDefault(v => v.Name == "Casablanca");
            if (casablancaCity != null)
            {
                var partnerAccounts = new List<PartnerAccount>
                {
                    PartnerAccount.Create(
                        new PartnerAccountId(Guid.NewGuid()),
                        "000123456789",
                        "12345678901234567890123",
                        "Casablanca Centre",
                        "Wafa Cash Services",
                        "WCS",
                        50000.00m,
                        banks.First(b => b.Code == "AWB"),
                        activityParamType
                    ),
                    PartnerAccount.Create(
                        new PartnerAccountId(Guid.NewGuid()),
                        "000987654321",
                        "98765432109876543210987",
                        "Casablanca Marina",
                        "Transfert Express",
                        "TE",
                        75000.00m,
                        banks.First(b => b.Code == "BMCE"),
                        commissionParamType
                    )
                };

                context.PartnerAccounts.AddRange(partnerAccounts);
                context.SaveChanges();
            }

           

            //Mettre en cache les données référentielles
            var referentiel = context.Countries
              .ToList();

            //await cacheService.SetAsync("Referentiel", referentiel, new TimeSpan(24));

            var casa = villes.First(v => v.Name == "Casablanca");
            var rabat = villes.First(v => v.Name == "Rabat");


            

            // TYPE-DEFINITION + PARAM-TYPES
            var agencyTypeDef = TypeDefinition.Create(
                TypeDefinitionId.Of(Guid.NewGuid()),
                libelle: "AgencyType",
                description: "Type of agency");

            var paramValues = new[]
            {
                "3G", "Kiosque", "Mobile", "Siège",
                "Siège partenaire", "Standard"
            };

            foreach (var v in paramValues)
            {
                var pt = ParamType.Create(
                    ParamTypeId.Of(Guid.NewGuid()),
                    agencyTypeDef!.Id!,
                    v
                    );

                agencyTypeDef.AddParamType(pt);
                context.ParamTypes.Add(pt);
            }

            context.TypeDefinitions.Add(agencyTypeDef);
            context.SaveChanges();

            var type3G = agencyTypeDef.ParamTypes.FirstOrDefault(p => p.Value == "3G");
            var typeStandard = agencyTypeDef.ParamTypes.FirstOrDefault(p => p.Value == "Standard");

            context.SaveChanges();

        }
    }
}