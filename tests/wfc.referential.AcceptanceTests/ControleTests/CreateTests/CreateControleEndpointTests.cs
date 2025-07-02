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
using wfc.referential.Domain.ControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ControleTests.CreateTests;

public class CreateControleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IControleRepository> _repoMock = new();

    public CreateControleEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IControleRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.AddAsync(It.IsAny<Controle>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Controle c, CancellationToken _) => c);

                _repoMock
                    .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    private static object ValidPayload(string? code = null, string? name = null) =>
        new
        {
            Code = code ?? $"C-{Guid.NewGuid():N}"[..6],
            Name = name ?? $"Controle-{Guid.NewGuid():N}"[..8]
        };

    private static Controle MakeControle(string code, string name) =>
        Controle.Create(ControleId.Of(Guid.NewGuid()), code, name);


    [Fact(DisplayName = "POST /api/controles → 200 & Guid on valid request")]
    public async Task Post_ShouldReturn201_AndId_WhenValid()
    {
        var payload = ValidPayload();

        var resp = await _client.PostAsJsonAsync("/api/controles", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        id.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<Controle>(c =>
                    c.Code == payload.GetType().GetProperty("Code")!.GetValue(payload)!.ToString() &&
                    c.Name == payload.GetType().GetProperty("Name")!.GetValue(payload)!.ToString()),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "POST /api/controles → 400 when Code missing")]
    public async Task Post_ShouldReturn400_WhenCodeMissing()
    {
        var bad = new { Name = "Missing-Code" };

        var resp = await _client.PostAsJsonAsync("/api/controles", bad);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("Code")[0].GetString()
            .Should().Be("Code is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Controle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/controles → 400 when Code > 50 chars")]
    public async Task Post_ShouldReturn400_WhenCodeTooLong()
    {
        var bad = ValidPayload(code: new string('X', 51));

        var resp = await _client.PostAsJsonAsync("/api/controles", bad);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("Code")[0].GetString()
            .Should().Be("Code max length = 50.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Controle>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "POST /api/controles → 400 when Name missing")]
    public async Task Post_ShouldReturn400_WhenNameMissing()
    {
        var bad = new { Code = "NONAME" };

        var resp = await _client.PostAsJsonAsync("/api/controles", bad);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("Name")[0].GetString()
            .Should().Be("Name is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Controle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/controles → 400 when Name > 100 chars")]
    public async Task Post_ShouldReturn400_WhenNameTooLong()
    {
        var bad = ValidPayload(name: new string('N', 101));

        var resp = await _client.PostAsJsonAsync("/api/controles", bad);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("Name")[0].GetString()
            .Should().Be("Name max length = 100.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Controle>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "POST /api/controles → 409 when Code already exists (case insensitive)")]
    public async Task Post_ShouldReturn409_WhenDuplicateCode()
    {
        const string dupCode = "DUPLICATE";

        _repoMock.Setup(r => r.GetOneByConditionAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Controle, bool>>>(),
                    It.IsAny<CancellationToken>()))
                 .ReturnsAsync(MakeControle(dupCode, "Whatever"));

        var resp = await _client.PostAsJsonAsync("/api/controles", ValidPayload(code: dupCode.ToLower()));
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Controle>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}