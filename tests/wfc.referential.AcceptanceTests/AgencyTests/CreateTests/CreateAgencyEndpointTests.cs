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
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.CreateTests;

public class CreateAgencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repo = new();

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

                _repo.Setup(r => r.AddAsync(It.IsAny<Agency>(),
                                            It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Agency a, CancellationToken _) => a);

                _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }


    private static Agency MakeAgency(string code) =>
        Agency.Create(
            id: AgencyId.Of(Guid.NewGuid()),
            code: code,
            name: "Existing",
            abbreviation: "EXI",
            address1: "addr",
            address2: null,
            phone: "000",
            fax: "",
            accountingSheetName: "sheet",
            accountingAccountNumber: "acc",
            postalCode: "10000",
            latitude: null,
            longitude: null,
            cashTransporter: null,
            expenseFundAccountingSheet: null,
            expenseFundAccountNumber: null,
            madAccount: null,
            fundingThreshold: null,
            cityId: CityId.Of(Guid.NewGuid()),
            sectorId: null,
            agencyTypeId: null,
            tokenUsageStatusId: null,
            fundingTypeId: null,
            partnerId: null,
            supportAccountId: null);


    [Fact(DisplayName = "POST /api/agencies returns 400 when Code not 6 digits")]
    public async Task Post_ShouldReturn400_WhenCodeInvalid()
    {
        var invalid = new
        {
            Code = "ABC99",
            Name = "Bad",
            Abbreviation = "BAD",
            Address1 = "Somewhere",
            Phone = "000",
            AccountingSheetName = "S",
            AccountingAccountNumber = "A",
            PostalCode = "1",
            CityId = Guid.NewGuid()
        };

        var resp = await _client.PostAsJsonAsync("/api/agencies", invalid);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
                .GetProperty("Code")[0]
                .GetString()
                .Should()
                .Be("Agency code must be exactly 6 digits when provided.");

        _repo.Verify(r => r.AddAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
                     Times.Never);
    }

    [Fact(DisplayName = "POST /api/agencies returns 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenDuplicateCode()
    {
        const string duplicate = "123456";
        _repo.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<Expression<Func<Agency, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeAgency(duplicate));

        var payload = new
        {
            Code = duplicate,
            Name = "Dup",
            Abbreviation = "DUP",
            Address1 = "X",
            Phone = "0",
            AccountingSheetName = "S",
            AccountingAccountNumber = "A",
            PostalCode = "1",
            CityId = Guid.NewGuid()
        };

        var resp = await _client.PostAsJsonAsync("/api/agencies", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        doc!.RootElement.GetProperty("errors")
                .GetProperty("message").GetString()
           .Should().Be($"Agency with code {duplicate} already exists.");

        _repo.Verify(r => r.AddAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
                     Times.Never);
    }
}