using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using wfc.referential.Application.AgencyTiers.Commands.UpdateAgencyTier;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.UpdateTests;

public class UpdateAgencyTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyTierRepository> _agencyTierRepoMock = new();
    private readonly Mock<IAgencyRepository> _agencyRepoMock = new();
    private readonly Mock<ITierRepository> _tierRepoMock = new();

    public UpdateAgencyTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAgencyTierRepository>();
                services.RemoveAll<IAgencyRepository>();
                services.RemoveAll<ITierRepository>();
                services.RemoveAll<ICacheService>();

                _agencyTierRepoMock
                    .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_agencyTierRepoMock.Object);
                services.AddSingleton(_agencyRepoMock.Object);
                services.AddSingleton(_tierRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static AgencyTier CreateAgencyTier(Guid id, Guid agencyId, Guid tierId,
                                               string code, string pwd, bool enabled = true)
    {
        var link = AgencyTier.Create(
            AgencyTierId.Of(id),
            AgencyId.Of(agencyId),
            TierId.Of(tierId),
            code,
            pwd);

        if (!enabled) link.Disable();
        return link;
    }

    private record BoolDto(bool Value);


    [Fact(DisplayName = "PUT /api/agencyTiers/{id} → 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateSuccessful()
    {
        // Arrange
        var linkId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var existing = CreateAgencyTier(linkId, agencyId, tierId, "OLD", "pwd");
        _agencyTierRepoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(linkId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(existing);

        // Agency & Tier existence checks
        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Agency)) as Agency);

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(tierId), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Tier)) as Tier);

        _agencyTierRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                                 System.Linq.Expressions.Expression<Func<AgencyTier, bool>>>(),
                                 It.IsAny<CancellationToken>()))
                           .ReturnsAsync((AgencyTier?)null); // code unique

        var payload = new
        {
            AgencyTierId = linkId,
            AgencyId = agencyId,
            TierId = tierId,
            Code = "NEW-CODE",
            Password = "newPwd",
            IsEnabled = false
        };

        // Act
        var res = await _client.PutAsJsonAsync($"/api/agencyTiers/{linkId}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        existing.Code.Should().Be("NEW-CODE");
        existing.Password.Should().Be("newPwd");
        existing.IsEnabled.Should().BeFalse();

        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/agencyTiers/{id} → 400 when Code > 30 chars")]
    public async Task Put_ShouldReturn400_WhenCodeTooLong()
    {
        var linkId = Guid.NewGuid();
        var longCode = new string('X', 31);

        var payload = new
        {
            AgencyTierId = linkId,
            AgencyId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Code = longCode
        };

        var res = await _client.PutAsJsonAsync($"/api/agencyTiers/{linkId}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
           .GetProperty("code")[0].GetString()
           .Should().Be("Code max length = 30.");

        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencyTiers/{id} → 404 when AgencyTier missing")]
    public async Task Put_ShouldReturn404_WhenAgencyTierNotFound()
    {
        var linkId = Guid.NewGuid();

        _agencyTierRepoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(linkId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((AgencyTier?)null);

        var payload = new UpdateAgencyTierCommand
        {
            AgencyTierId = linkId,
            AgencyId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Code = "SOME"
        };

        var res = await _client.PutAsJsonAsync($"/api/agencyTiers/{linkId}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "PUT /api/agencyTiers/{id} → 404 when Agency or Tier missing")]
    [InlineData("Agency")]
    [InlineData("Tier")]
    public async Task Put_ShouldReturn404_WhenRelatedEntityMissing(string missing)
    {
        var linkId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var link = CreateAgencyTier(linkId, agencyId, tierId, "C", "pwd");
        _agencyTierRepoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(linkId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(link);

        if (missing == "Agency")
        {
            _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Agency?)null);
            _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(tierId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Tier)) as Tier);
        }
        else
        {
            _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Agency)) as Agency);
            _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(tierId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Tier?)null);
        }

        var payload = new
        {
            AgencyTierId = linkId,
            AgencyId = agencyId,
            TierId = tierId,
            Code = "NEW",
            Password = "pwd"
        };

        var res = await _client.PutAsJsonAsync($"/api/agencyTiers/{linkId}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencyTiers/{id} → 409 when duplicate Code exists")]
    public async Task Put_ShouldReturn409_WhenDuplicateCode()
    {
        var linkId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        const string dupCode = "DUP";

        var link = CreateAgencyTier(linkId, agencyId, tierId, "OLD", "pwd");
        var otherLink = CreateAgencyTier(Guid.NewGuid(), agencyId, tierId, dupCode, "pwd2");

        _agencyTierRepoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(linkId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(link);

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Agency)) as Agency);

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(tierId), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Tier)) as Tier);

        _agencyTierRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                                 System.Linq.Expressions.Expression<Func<AgencyTier, bool>>>(),
                                 It.IsAny<CancellationToken>()))
                           .ReturnsAsync(otherLink); 

        var payload = new
        {
            AgencyTierId = linkId,
            AgencyId = agencyId,
            TierId = tierId,
            Code = dupCode
        };

        var res = await _client.PutAsJsonAsync($"/api/agencyTiers/{linkId}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencyTiers/{id} → 200 when disabling link")]
    public async Task Put_ShouldReturn200_WhenDisablingLink()
    {
        var linkId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var link = CreateAgencyTier(linkId, agencyId, tierId, "C", "pwd", enabled: true);

        _agencyTierRepoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(linkId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(link);

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Agency)) as Agency);

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(tierId), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Tier)) as Tier);

        _agencyTierRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                                 System.Linq.Expressions.Expression<Func<AgencyTier, bool>>>(),
                                 It.IsAny<CancellationToken>()))
                           .ReturnsAsync((AgencyTier?)null);

        var payload = new
        {
            AgencyTierId = linkId,
            AgencyId = agencyId,
            TierId = tierId,
            Code = "C",
            Password = "pwd",
            IsEnabled = false
        };

        var res = await _client.PutAsJsonAsync($"/api/agencyTiers/{linkId}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        link.IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "PUT /api/agencyTiers/{id} → 200 when keeping same Code")]
    public async Task Put_ShouldReturn200_WhenKeepingSameCode()
    {
        var linkId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var link = CreateAgencyTier(linkId, agencyId, tierId, "SAME", "pwd");

        _agencyTierRepoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(linkId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(link);

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Agency)) as Agency);

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(tierId), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Tier)) as Tier);

        _agencyTierRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                                 System.Linq.Expressions.Expression<Func<AgencyTier, bool>>>(),
                                 It.IsAny<CancellationToken>()))
                           .ReturnsAsync(link);

        var payload = new
        {
            AgencyTierId = linkId,
            AgencyId = agencyId,
            TierId = tierId,
            Code = "SAME",
            Password = "newPwd"
        };

        var res = await _client.PutAsJsonAsync($"/api/agencyTiers/{linkId}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        link.Password.Should().Be("newPwd");
        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}