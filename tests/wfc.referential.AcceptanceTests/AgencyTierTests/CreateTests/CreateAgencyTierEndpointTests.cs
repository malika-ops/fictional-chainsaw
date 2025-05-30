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
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.CreateTests;

public class CreateAgencyTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyTierRepository> _agencyTierRepoMock = new();
    private readonly Mock<IAgencyRepository> _agencyRepoMock = new();
    private readonly Mock<ITierRepository> _tierRepoMock = new();

    public CreateAgencyTierEndpointTests(WebApplicationFactory<Program> factory)
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
                    .Setup(r => r.AddAsync(It.IsAny<AgencyTier>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((AgencyTier at, CancellationToken _) => at);

                _agencyTierRepoMock
                    .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var dummyAgency = FormatterServices.GetUninitializedObject(typeof(Agency)) as Agency;
                var dummyTier = FormatterServices.GetUninitializedObject(typeof(Tier)) as Tier;

                _agencyRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<AgencyId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dummyAgency);

                _tierRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<TierId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dummyTier);

                services.AddSingleton(_agencyTierRepoMock.Object);
                services.AddSingleton(_agencyRepoMock.Object);
                services.AddSingleton(_tierRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }


    [Fact(DisplayName = "POST /api/agencyTiers → 200 + Guid on valid request")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            AgencyId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Code = "AG-TIER-001",
            Password = "s3cr3t"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/agencyTiers", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _agencyTierRepoMock.Verify(r =>
            r.AddAsync(It.Is<AgencyTier>(at =>
                    at.AgencyId.Value == payload.AgencyId &&
                    at.TierId.Value == payload.TierId &&
                    at.Code == payload.Code &&
                    at.Password == payload.Password),
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                                   Times.Once);
    }


    [Fact(DisplayName = "POST /api/agencyTiers → 400 when Code exceeds 30 chars")]
    public async Task Post_ShouldReturn400_WhenCodeTooLong()
    {
        var payload = new
        {
            AgencyId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Code = new string('X', 31)
        };

        var response = await _client.PostAsJsonAsync("/api/agencyTiers", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code max length = 30.");

        _agencyTierRepoMock.Verify(r => r.AddAsync(It.IsAny<AgencyTier>(),
                                                   It.IsAny<CancellationToken>()),
                                   Times.Never);
    }

    [Fact(DisplayName = "POST /api/agencyTiers → 400 when duplicate Code for same Agency & Tier")]
    public async Task Post_ShouldReturn400_WhenDuplicateCode()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        const string code = "DUP-123";

        // Existing record returned by repo to trigger DuplicateAgencyTierCodeException
        var existing = AgencyTier.Create(
            AgencyTierId.Of(Guid.NewGuid()),
            AgencyId.Of(agencyId),
            TierId.Of(tierId),
            code,
            null);

        _agencyTierRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<
                    System.Linq.Expressions.Expression<Func<AgencyTier, bool>>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var payload = new { AgencyId = agencyId, TierId = tierId, Code = code };

        // Act
        var response = await _client.PostAsJsonAsync("/api/agencyTiers", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _agencyTierRepoMock.Verify(r => r.AddAsync(It.IsAny<AgencyTier>(),
                                                   It.IsAny<CancellationToken>()),
                                   Times.Never);
    }

    [Fact(DisplayName = "POST /api/agencyTiers → 404 when Agency does not exist")]
    public async Task Post_ShouldReturn404_WhenAgencyNotFound()
    {
        _agencyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<AgencyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Agency?)null); // simulate missing agency

        var payload = new
        {
            AgencyId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Code = "CODE-404"
        };

        var response = await _client.PostAsJsonAsync("/api/agencyTiers", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyTierRepoMock.Verify(r => r.AddAsync(It.IsAny<AgencyTier>(),
                                                   It.IsAny<CancellationToken>()),
                                   Times.Never);
    }

    [Fact(DisplayName = "POST /api/agencyTiers → 404 when Tier does not exist")]
    public async Task Post_ShouldReturn404_WhenTierNotFound()
    {
        var dummyAgency = FormatterServices.GetUninitializedObject(typeof(Agency)) as Agency;
        _agencyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<AgencyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyAgency);

        _tierRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TierId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tier?)null); // simulate missing tier

        var payload = new
        {
            AgencyId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Code = "CODE-404"
        };

        var response = await _client.PostAsJsonAsync("/api/agencyTiers", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyTierRepoMock.Verify(r => r.AddAsync(It.IsAny<AgencyTier>(),
                                                   It.IsAny<CancellationToken>()),
                                   Times.Never);
    }
}