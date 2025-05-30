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
using System.Text;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.PatchTests;

public class PatchAgencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _agencyRepo = new();
    private readonly Mock<ICityRepository> _cityRepo = new();
    private readonly Mock<ISectorRepository> _sectorRepo = new();
    private readonly Mock<IParamTypeRepository> _paramRepo = new();
    private readonly Mock<IPartnerRepository> _partnerRepo = new();
    private readonly Mock<ISupportAccountRepository> _supportRepo = new();

    public PatchAgencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyRepository>();
                s.RemoveAll<ICityRepository>();
                s.RemoveAll<ISectorRepository>();
                s.RemoveAll<IParamTypeRepository>();
                s.RemoveAll<IPartnerRepository>();
                s.RemoveAll<ISupportAccountRepository>();
                s.RemoveAll<ICacheService>();

                _agencyRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

                s.AddSingleton(_agencyRepo.Object);
                s.AddSingleton(_cityRepo.Object);
                s.AddSingleton(_sectorRepo.Object);
                s.AddSingleton(_paramRepo.Object);
                s.AddSingleton(_partnerRepo.Object);
                s.AddSingleton(_supportRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }


    private static Agency MakeAgency(Guid id, string code = "ABC123") =>
        Agency.Create(
            AgencyId.Of(id), code, "Agency Name", "AGN",
            "1 Main St", null,
            phone: "0600000000", fax: "",
            accountingSheetName: "Sheet", accountingAccountNumber: "401122",
            postalCode: "10000",
            latitude: null, longitude: null,
            cashTransporter: null,
            expenseFundAccountingSheet: null,
            expenseFundAccountNumber: null,
            madAccount: null,
            fundingThreshold: null,
            cityId: CityId.Of(Guid.NewGuid()),   // satisfy XOR rule
            sectorId: null,
            agencyTypeId: null,
            tokenUsageStatusId: null,
            fundingTypeId: null,
            partnerId: null,
            supportAccountId: null);

    private static async Task<HttpResponseMessage> PatchJsonAsync(
        HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await client.SendAsync(req);
    }

    private static async Task<bool> ReadBoolAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;

        if (root.ValueKind == JsonValueKind.True || root.ValueKind == JsonValueKind.False)
            return root.GetBoolean();

        if (root.TryGetProperty("value", out var v) &&
            (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False))
            return v.GetBoolean();

        return root.GetBoolean();    
    }

    private static string FirstErr(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }


    [Fact(DisplayName = "PATCH /api/agencies/{id} returns 200 when patch succeeds")]
    public async Task Patch_ShouldReturn200_WhenPatchSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orig = MakeAgency(id);

        _agencyRepo.Setup(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(orig);

        _agencyRepo.Setup(r => r.GetOneByConditionAsync(
                                It.IsAny<Expression<Func<Agency, bool>>>(),
                                It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Agency?)null);

        Agency? saved = null;
        _agencyRepo.Setup(r => r.Update(It.IsAny<Agency>()))
                   .Callback<Agency>(a => saved = a);

        var payload = new
        {
            AgencyId = id,
            Phone = "0611111111",
            CashTransporter = "BRINKS"
        };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/agencies/{id}", payload);
        var result = await ReadBoolAsync(resp);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        saved!.Phone.Should().Be("0611111111");
        saved.CashTransporter.Should().Be("BRINKS");
        saved.Name.Should().Be("Agency Name");    // unchanged

        _agencyRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/agencies/{id} returns 400 when agency not found")]
    public async Task Patch_ShouldReturn400_WhenAgencyMissing()
    {
        var id = Guid.NewGuid();

        _agencyRepo.Setup(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Agency?)null);

        var payload = new { AgencyId = id, Name = "Nope" };

        var resp = await PatchJsonAsync(_client, $"/api/agencies/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/agencies/{id} returns 400 when new Code duplicates another agency")]
    public async Task Patch_ShouldReturn400_WhenCodeDuplicate()
    {
        var idTarget = Guid.NewGuid();
        var target = MakeAgency(idTarget, "ABC123");
        var dup = MakeAgency(Guid.NewGuid(), "XYZ999");

        _agencyRepo.Setup(r => r.GetByIdAsync(AgencyId.Of(idTarget), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(target);

        _agencyRepo.Setup(r => r.GetOneByConditionAsync(
                                It.IsAny<Expression<Func<Agency, bool>>>(),
                                It.IsAny<CancellationToken>()))
                   .ReturnsAsync(dup);

        var payload = new { AgencyId = idTarget, Code = "XYZ999" };

        var resp = await PatchJsonAsync(_client, $"/api/agencies/{idTarget}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Agency with code XYZ999 already exists.");

        _agencyRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/agencies/{id} returns 400 when AgencyId is empty GUID")]
    public async Task Patch_ShouldReturn400_WhenIdEmpty()
    {
        var body = new { AgencyId = Guid.Empty, Name = "Fails" };

        var resp = await PatchJsonAsync(
            _client,
            "/api/agencies/00000000-0000-0000-0000-000000000000",
            body);

        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstErr(doc!.RootElement.GetProperty("errors"), "AgencyId")
            .Should().Be("AgencyId cannot be empty.");

        _agencyRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}