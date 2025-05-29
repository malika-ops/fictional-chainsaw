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
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.CreateTests;

public class CreateTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITierRepository> _repoMock = new();

    public CreateTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                /*  replace real infra with mocks  */
                s.RemoveAll<ITierRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.AddAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Tier t, CancellationToken _) => t);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /* ----------------------------------------------------------------
       1) Happy-path — valid request returns 200 + generated Guid
       ----------------------------------------------------------------*/
    [Fact(DisplayName = "POST /api/tiers returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        var payload = new
        {
            Name = "Silver",
            Description = "Basic tier"
        };

        var resp = await _client.PostAsJsonAsync("/api/tiers", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<Tier>(t =>
                    t.Name == payload.Name &&
                    t.Description == payload.Description &&
                    t.IsEnabled),                   // default true
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /* ----------------------------------------------------------------
       2) Validation — Name missing
       ----------------------------------------------------------------*/
    [Fact(DisplayName = "POST /api/tiers returns 400 when Name is missing")]
    public async Task Post_ShouldReturn400_WhenNameMissing()
    {
        var invalid = new   // Name omitted ➜ validation error
        {
            Description = "No name"
        };

        var resp = await _client.PostAsJsonAsync("/api/tiers", invalid);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Tier>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    /* ----------------------------------------------------------------
       3) Validation — Name > 200 chars
       ----------------------------------------------------------------*/
    [Fact(DisplayName = "POST /api/tiers returns 400 when Name exceeds 200 chars")]
    public async Task Post_ShouldReturn400_WhenNameTooLong()
    {
        var tooLong = new string('X', 201);

        var invalid = new
        {
            Name = tooLong,
            Description = "desc"
        };

        var resp = await _client.PostAsJsonAsync("/api/tiers", invalid);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name max length is 200 chars.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Tier>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    /* ----------------------------------------------------------------
       4) Business-rule — duplicate Name
       ----------------------------------------------------------------*/
    [Fact(DisplayName = "POST /api/tiers returns 400 when Name already exists")]
    public async Task Post_ShouldReturn400_WhenNameAlreadyExists()
    {
        const string duplicate = "Gold";


        var payload = new
        {
            Name = duplicate,
            Description = "should fail"
        };

        var resp = await _client.PostAsJsonAsync("/api/tiers", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
            .Should().Be($"Tier '{duplicate}' already exists.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Tier>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
