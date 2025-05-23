using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.CreateTests;

public class CreateAgencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repoMock = new();

    public CreateAgencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.AddAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Agency a, CancellationToken _) => a);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }


    // Happy-path — valid request returns 200 + GUID
    [Fact(DisplayName = "POST /api/agencies returns 200 and Guid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        var payload = new
        {
            Code = "AGX01",
            Name = "Agency X",
            Abbreviation = "AGX",
            Address1 = "1 Main St",
            Phone = "0600000000",
            AccountingSheetName = "Sheet-01",
            AccountingAccountNumber = "401122",
            PostalCode = "10000"

        };

        var resp = await _client.PostAsJsonAsync("/api/agencies", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _repoMock.Verify(r =>
             r.AddAsync(It.Is<Agency>(a =>
                 a.Code == payload.Code &&
                 a.Name == payload.Name &&
                 a.Abbreviation == payload.Abbreviation),
                 It.IsAny<CancellationToken>()),
             Times.Once);
    }


    // Missing Code -> 400 + validation problem-details
    [Fact(DisplayName = "POST /api/agencies returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeMissing()
    {
        var invalid = new
        {
            Name = "Agency X",
            Abbreviation = "AGX",
            Address1 = "1 Main St",
            Phone = "0600000000",
            AccountingSheetName = "Sheet-01",
            AccountingAccountNumber = "401122",
            PostalCode = "10000"
        };

        var resp = await _client.PostAsJsonAsync("/api/agencies", invalid);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Agency code is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Agency>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }


     // Missing Name & Address1 -> 400 + both errors
    [Fact(DisplayName = "POST /api/agencies returns 400 when Name and Address1 are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndAddressMissing()
    {
        var invalid = new
        {
            Code = "AGX01",
            Abbreviation = "AGX",
            Phone = "0600000000",
            AccountingSheetName = "Sheet-01",
            AccountingAccountNumber = "401122",
            PostalCode = "10000"
        };

        var resp = await _client.PostAsJsonAsync("/api/agencies", invalid);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errs = doc!.RootElement.GetProperty("errors");
        errs.GetProperty("name")[0].GetString().Should().Be("Agency Name is required.");
        errs.GetProperty("address1")[0].GetString().Should().Be("Address is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Agency>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

     // Duplicate Code -> 400 business rule error
    [Fact(DisplayName = "POST /api/agencies returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        const string duplicate = "AGD01";

        var existing = Agency.Create(
            AgencyId.Of(Guid.NewGuid()), duplicate, "Old Agency", "OLD",
            "addr", null, "phone", "", "sheet", "acc", "", "", "10000", "",
            null, null,  null, null, null, null, null);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var payload = new
        {
            Code = duplicate,
            Name = "Another Agency",
            Abbreviation = "ANA",
            Address1 = "somewhere",
            Phone = "0600000000",
            AccountingSheetName = "sheet",
            AccountingAccountNumber = "401122",
            PostalCode = "10000"
        };

        var resp = await _client.PostAsJsonAsync("/api/agencies", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
            .Should().Be($"Agency with code {duplicate} already exists.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Agency>(),
                                         It.IsAny<CancellationToken>()),
        Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}