using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.CreateTests;

public class CreatePartnerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();
    private readonly Mock<IPartnerAccountRepository> _partnerAccountRepoMock = new();
    private readonly Mock<ISupportAccountRepository> _supportAccountRepoMock = new();

    public CreatePartnerEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Partner p, CancellationToken _) => p);

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_partnerAccountRepoMock.Object);
                services.AddSingleton(_supportAccountRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/partners returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        Partner capturedCreatePartner = null;
        _repoMock
            .Setup(r => r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()))
            .Callback<Partner, CancellationToken>((p, _) => capturedCreatePartner = p)
            .ReturnsAsync((Partner p, CancellationToken _) => p);

        _repoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner)null);

        _repoMock
            .Setup(r => r.GetByIdentificationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner)null);

        _repoMock
            .Setup(r => r.GetByICEAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner)null);

        var payload = new
        {
            Code = "PTN001",
            Label = "Partner 1",
            Type = "3G - Kiosque - Mobile",
            NetworkMode = "Franchise",
            PaymentMode = "PrePaye",
            SupportAccountType = "Commun",
            TaxIdentificationNumber = "ID12345",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE12345",
            RASRate = "10.5",
            Logo = "/logos/logo.png",
            IdParent = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Vérifier que la méthode a été appelée
        _repoMock.Verify(r => r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Once);

        // Assertions basées sur ce que nous savons être présent
        capturedCreatePartner.Should().NotBeNull();
        capturedCreatePartner.Code.Should().Be(payload.Code);
        capturedCreatePartner.Label.Should().Be(payload.Label);
        capturedCreatePartner.Type.Should().Be(payload.Type);
        capturedCreatePartner.NetworkMode.ToString().Should().Be(payload.NetworkMode);
        capturedCreatePartner.PaymentMode.ToString().Should().Be(payload.PaymentMode);
        capturedCreatePartner.TaxIdentificationNumber.Should().Be(payload.TaxIdentificationNumber);
        capturedCreatePartner.ICE.Should().Be(payload.ICE);
        capturedCreatePartner.RASRate.Should().Be(payload.RASRate);
        capturedCreatePartner.SupportAccountType.ToString().Should().Be(payload.SupportAccountType);
    }

    [Fact(DisplayName = "POST /api/partners returns 400 & problem-details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var invalidPayload = new
        {
            // Code intentionally omitted to trigger validation error
            Label = "Partner 1",
            Type = "3G - Kiosque - Mobile",
            NetworkMode = "Franchise",
            PaymentMode = "PrePaye",
            SupportAccountType = "Commun",
            TaxIdentificationNumber = "ID12345",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE12345",
            RASRate = "10.5",
            Logo = "/logos/logo.png",
            IdParent = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "PTN001";

        // Tell the repo mock that the code already exists
        var existingPartner = CreateTestPartner(duplicateCode, "Existing Partner", NetworkMode.Franchise, "IDNUM123");

        _repoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPartner);

        var payload = new
        {
            Code = duplicateCode,
            Label = "New Partner",
            Type = "3G - Kiosque - Mobile",
            NetworkMode = "Succursale",
            PaymentMode = "PostPaye",
            SupportAccountType = "Individuel",
            TaxIdentificationNumber = "ID67890",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE67890",
            RASRate = "15.2",
            Logo = "/logos/new.png",
            IdParent = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Partner with code {duplicateCode} already exists.");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }

    // Helper to build dummy partners quickly
    private static Partner CreateTestPartner(string code, string label, NetworkMode networkMode, string taxIdentificationNumber)
    {
        return Partner.Create(
            PartnerId.Of(Guid.NewGuid()),
            code,
            label,
            networkMode,
            PaymentMode.PrePaye,
            "Test Type",
            Domain.SupportAccountAggregate.SupportAccountType.Commun,
            taxIdentificationNumber,
            "Standard",
            "AUX" + code,
            "ICE" + code,
            "10.5",
            "/logos/logo.png",
            null, // IdParent
            null, // CommissionAccountId
            null, // ActivityAccountId
            null  // SupportAccountId
        );
    }
}