using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.UpdateTests;

public class UpdateAgencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repoMock = new();

    public UpdateAgencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAgencyRepository>();
                services.RemoveAll<ICacheService>();

                //_repoMock.Setup(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()))
                //         .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    // helpers

    private static Agency Ag(string code, string name, Guid id)
    {
        // using first 3 chars (or fewer) as abbreviation
        var abbr = code.Length >= 3 ? code[..3] : code;

        return Agency.Create(
            AgencyId.Of(id),
            code,                        // Code
            name,                        // Name
            abbr,                        // Abbreviation
            "1 Main St",                 // Address1
            null,                        // Address2
            "0600000000",                // Phone
            "",                          // Fax
            "Sheet",                     // AccountingSheetName
            "401122",                    // AccountingAccountNumber
            "", "",                      // MoneyGram ref/pwd
            "10000",                     // Postal
            "",                          // PermissionOfficeChange
            null, null,                  // lat / long
            CityId.Of(Guid.NewGuid()),   // CityId
            null,                        // SectorId
            null,                        // AgencyTypeId
            null, null);                 // SupportAccountId / PartnerId
    }

    // happy-path

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var old = Ag("AGD1", "Old Name", id);

        //_repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(old);

        //_repoMock.Setup(r => r.GetByCodeAsync("NEW1", It.IsAny<CancellationToken>()))
        //         .ReturnsAsync((Agency?)null);    

        //Agency? updated = null;
        //_repoMock.Setup(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()))
        //         .Callback<Agency, CancellationToken>((a, _) => updated = a)
        //         .Returns(Task.CompletedTask);

        var payload = new
        {
            AgencyId = id,
            Code = "NEW1",
            Name = "New Agency",
            Abbreviation = "NEW",
            Address1 = "99 Broadway",
            Phone = "0612345678",
            AccountingSheetName = "NEW-SHEET",
            AccountingAccountNumber = "701122",
            PostalCode = "90000",
            CityId = Guid.NewGuid()         // exactly one of city/sector
        };

        // Act
        var resp = await _client.PutAsJsonAsync($"/api/agencies/{id}", payload);
        var result = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        //updated!.Code.Should().Be("NEW1");
        //updated.Name.Should().Be("New Agency");
        //updated.Address1.Should().Be("99 Broadway");
        //updated.AccountingAccountNumber.Should().Be("701122");

        //_repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
        //                 Times.Once);
    }

    // validation error (Name missing)

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            AgencyId = id,
            Code = "AGX",
            // Name omitted
            Abbreviation = "AGX",
            Address1 = "1 Main",
            Phone = "0600000000",
            AccountingSheetName = "SH",
            AccountingAccountNumber = "100",
            PostalCode = "10000",
            CityId = Guid.NewGuid()
        };

        // Act
        var resp = await _client.PutAsJsonAsync($"/api/agencies/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required.");

        //_repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
        //                 Times.Never);
    }

    // duplicate code

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 400 when new Code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var duplicate = Ag("DUP1", "Duplicate", Guid.NewGuid());
        var target = Ag("OLD1", "Target", id);

        //_repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(target);

        //_repoMock.Setup(r => r.GetByCodeAsync("DUP1", It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(duplicate);       

        var payload = new
        {
            AgencyId = id,
            Code = "DUP1",   // duplicate code
            Name = "Target Updated",
            Abbreviation = "TAR",
            Address1 = "X",
            Phone = "0",
            AccountingSheetName = "S",
            AccountingAccountNumber = "A",
            PostalCode = "1",
            CityId = Guid.NewGuid()
        };

        // Act
        var resp = await _client.PutAsJsonAsync($"/api/agencies/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Agency with code DUP1 already exists.");

        //_repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
        //                 Times.Never);
    }

    // both CityId & SectorId supplied (mutual-exclusion)

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 400 when both CityId and SectorId are provided")]
    public async Task Put_ShouldReturn400_WhenCityAndSectorProvided()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            AgencyId = id,
            Code = "CS1",
            Name = "CitySector",
            Abbreviation = "CS1",
            Address1 = "1 Main",
            Phone = "0600000000",
            AccountingSheetName = "SH",
            AccountingAccountNumber = "100",
            PostalCode = "10000",
            CityId = Guid.NewGuid(),    // both set
            SectorId = Guid.NewGuid()
        };

        // Act
        var resp = await _client.PutAsJsonAsync($"/api/agencies/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
           .GetProperty("")[0]          // property-level error (no field)
           .GetString()
           .Should().Be("Exactly one of CityId or SectorId must be supplied.");

        //_repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
        //                 Times.Never);
    }
}