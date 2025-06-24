using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Audit.Interface;
using BuildingBlocks.Core.Kafka.Producer;
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
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.CreateTests;

public class CreateMonetaryZoneEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IMonetaryZoneRepository> _repoMock = new();
    private readonly Mock<IProducerService> _kafkaMock = new();
    private readonly Mock<ICurrentUserContext> _userCtx = new();

    public CreateMonetaryZoneEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IMonetaryZoneRepository>();
                s.RemoveAll<IProducerService>();
                s.RemoveAll<ICurrentUserContext>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.AddAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((MonetaryZone m, CancellationToken _) => m);

                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                _kafkaMock.Setup(p => p.ProduceAsync(It.IsAny<object>(), It.IsAny<string>()))
                          .Returns(Task.CompletedTask);

                _userCtx.SetupGet(u => u.UserId).Returns("test-user");
                _userCtx.SetupGet(u => u.TraceId).Returns(Guid.NewGuid().ToString());

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(_kafkaMock.Object);
                s.AddSingleton(_userCtx.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    private static object ValidPayload(string? code = null,
                                       string? name = null,
                                       string? description = null)
    {
        return new
        {
            Code = code ?? "EU",
            Name = name ?? "Europe",
            Description = description ?? "European monetary zone"
        };
    }


    [Fact(DisplayName = "POST /api/monetaryZones → 200 & Guid on valid request")]
    public async Task Post_ShouldReturn200_AndId_WhenValid()
    {
        // Arrange
        var payload = ValidPayload();
        var code = "EU"; var name = "Europe"; var desc = "European monetary zone";

        // Act
        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<MonetaryZone>(m =>
                    m.Code == code &&
                    m.Name == name &&
                    m.Description == desc),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _kafkaMock.Verify(p => p.ProduceAsync(It.IsAny<object>(), "auditLogsTopic"), Times.Once);
    }

    [Fact(DisplayName = "POST /api/monetaryZones → 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenDuplicateCode()
    {
        // Arrange
        const string dup = "EU";
        var existing = MonetaryZone.Create(
            MonetaryZoneId.Of(Guid.NewGuid()), dup, "Old-Europe", "Desc");

        _repoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", ValidPayload(code: dup));

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/monetaryZones → 400 when Code exceeds 20 chars")]
    public async Task Post_ShouldReturn400_WhenCodeTooLong()
    {
        var longCode = new string('X', 21);
        var payload = ValidPayload(code: longCode);

        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
           .GetProperty("Code")[0].GetString()
           .Should().Be("Code must be less than 10 characters");   

        _repoMock.Verify(r => r.AddAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/monetaryZones → 400 when Name missing")]
    public async Task Post_ShouldReturn400_WhenNameMissing()
    {
        var payload = new { Code = "EU" };   

        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
           .GetProperty("Name")[0].GetString()
           .Should().Be("Name is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/monetaryZones → 200 when Description omitted")]
    public async Task Post_ShouldReturn200_WhenDescriptionOmitted()
    {
        // Arrange
        var code = "AF";
        var name = "Africa";
        var payload = new { Code = code, Name = name };  

        // Act
        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<MonetaryZone>(m =>
                    m.Code == code &&
                    m.Name == name &&
                    m.Description == string.Empty),   
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}