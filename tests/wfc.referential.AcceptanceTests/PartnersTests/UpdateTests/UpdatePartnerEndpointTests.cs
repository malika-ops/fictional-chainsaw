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

namespace wfc.referential.AcceptanceTests.PartnersTests.UpdateTests;

public class UpdatePartnerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();
    private readonly Mock<IPartnerAccountRepository> _partnerAccountRepoMock = new();
    private readonly Mock<ISupportAccountRepository> _supportAccountRepoMock = new();

    public UpdatePartnerEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_partnerAccountRepoMock.Object);
                services.AddSingleton(_supportAccountRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to create a test partner
    private static Partner CreateTestPartner(Guid id, string code, string label, NetworkMode networkMode)
    {
        return Partner.Create(
            new PartnerId(id),
            code,
            label,
            networkMode,
            PaymentMode.PrePaye,
            "Test Type",
            Domain.SupportAccountAggregate.SupportAccountType.Commun,
            "IDNUM" + code,
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

    [Fact(DisplayName = "PUT /api/partners/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldPartner = CreateTestPartner(id, "PTN001", "Old Partner", NetworkMode.Franchise);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);

        _repoMock.Setup(r => r.GetByCodeAsync("PTN002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);   // Code is unique

        _repoMock.Setup(r => r.GetByIdentificationNumberAsync("IDNUM002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);   // Identification number is unique

        _repoMock.Setup(r => r.GetByICEAsync("ICE002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);   // ICE is unique

        Partner? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                        It.IsAny<CancellationToken>()))
                 .Callback<Partner, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            Code = "PTN002",
            Label = "New Partner Name",
            Type = "3G - Kiosque - Mobile",
            NetworkMode = "Succursale",
            PaymentMode = "PostPaye",
            SupportAccountType = "Individuel",
            TaxIdentificationNumber = "IDNUM002",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE002",
            RASRate = "12.3",
            Logo = "/logos/new-logo.png",
            IdParent = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("PTN002");
        updated.Label.Should().Be("New Partner Name");
        updated.Type.Should().Be("3G - Kiosque - Mobile");
        updated.NetworkMode.Should().Be(NetworkMode.Succursale);
        updated.PaymentMode.Should().Be(PaymentMode.PostPaye);
        updated.TaxIdentificationNumber.Should().Be("IDNUM002");
        updated.TaxRegime.Should().Be("Simplified");
        updated.AuxiliaryAccount.Should().Be("AUX002");
        updated.ICE.Should().Be("ICE002");
        updated.RASRate.Should().Be("12.3");
        updated.Logo.Should().Be("/logos/new-logo.png");
        updated.SupportAccountType.Should().Be(Domain.SupportAccountAggregate.SupportAccountType.Individuel);
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} allows changing the enabled status")]
    public async Task Put_ShouldAllowChangingEnabledStatus_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldPartner = CreateTestPartner(id, "PTN001", "Test Partner", NetworkMode.Franchise);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);

        _repoMock.Setup(r => r.GetByCodeAsync("PTN001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);   // Same code is ok for same partner

        _repoMock.Setup(r => r.GetByIdentificationNumberAsync("IDNUMPTN001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);   // Same identification number is ok for same partner

        _repoMock.Setup(r => r.GetByICEAsync("ICEPTN001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);   // Same ICE is ok for same partner

        Partner? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                        It.IsAny<CancellationToken>()))
                 .Callback<Partner, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            Code = "PTN001",
            Label = "Test Partner",
            Type = "Test Type",
            NetworkMode = "Franchise",
            PaymentMode = "PrePaye",
            SupportAccountType = "Commun",
            TaxIdentificationNumber = "IDNUMPTN001",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUXPTN001",
            ICE = "ICEPTN001",
            RASRate = "10.5",
            Logo = "/logos/logo.png",
            IdParent = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IsEnabled = false // Changed from true to false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 400 when Code is missing")]
    public async Task Put_ShouldReturn400_WhenCodeMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            PartnerId = id,
            // Code intentionally omitted
            Label = "New Partner Name",
            Type = "3G - Kiosque - Mobile",
            NetworkMode = "Succursale",
            PaymentMode = "PostPaye",
            SupportAccountType = "Individuel",
            TaxIdentificationNumber = "IDNUM002",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE002",
            RASRate = "12.3",
            Logo = "/logos/new-logo.png",
            IdParent = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 400 when Partner doesn't exist")]
    public async Task Put_ShouldReturn404_WhenPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);

        var payload = new
        {
            PartnerId = id,
            Code = "PTN002",
            Label = "New Partner",
            Type = "3G - Kiosque - Mobile",
            NetworkMode = "Succursale",
            PaymentMode = "PostPaye",
            SupportAccountType = "Individuel",
            TaxIdentificationNumber = "IDNUM002",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE002",
            RASRate = "12.3",
            Logo = "/logos/new-logo.png",
            IdParent = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Partner with ID {id} not found");

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}