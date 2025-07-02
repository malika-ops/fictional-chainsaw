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

namespace wfc.referential.AcceptanceTests.ControleTests.UpdateTests;

public class UpdateControleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IControleRepository> _repo = new();

    public UpdateControleEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(services =>
            {
                services.RemoveAll<IControleRepository>();
                services.RemoveAll<ICacheService>();

                _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

                services.AddSingleton(_repo.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Controle Make(Guid id, string code = "OLD", string name = "Old-Name", bool enabled = true)
    {
        var c = Controle.Create(ControleId.Of(id), code, name);
        if (!enabled) c.Disable();
        return c;
    }

    private static async Task<bool> ReadBool(HttpResponseMessage r)
        => (await r.Content.ReadFromJsonAsync<bool>());


    [Fact(DisplayName = "PUT /api/controles/{id} → 200 on successful update")]
    public async Task Put_ShouldReturn200_WhenSuccess()
    {
        var id = Guid.NewGuid();
        var entity = Make(id);
        _repo.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);
        _repo.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Controle, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync((Controle?)null);   

        var payload = new
        {
            ControleId = id,
            Code = "NEW",
            Name = "New-Name",
            IsEnabled = false
        };

        var res = await _client.PutAsJsonAsync($"/api/controles/{id}", payload);
        var ok = await ReadBool(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        entity.Code.Should().Be("NEW");
        entity.Name.Should().Be("New-Name");
        entity.IsEnabled.Should().BeFalse();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT keeps same Code → 200")]
    public async Task Put_ShouldReturn200_WhenKeepingSameCode()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, code: "SAME");
        _repo.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);
        _repo.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Controle, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);         

        var payload = new
        {
            ControleId = id,
            Code = "SAME",
            Name = "Changed",
            IsEnabled = true
        };

        var res = await _client.PutAsJsonAsync($"/api/controles/{id}", payload);
        var ok = await ReadBool(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        entity.Name.Should().Be("Changed");
    }


    [Fact(DisplayName = "PUT → 400 when Code > 50 chars")]
    public async Task Put_ShouldReturn400_WhenCodeTooLong()
    {
        var id = Guid.NewGuid();
        var longCode = new string('X', 51);

        var payload = new { ControleId = id, Code = longCode, Name = "N" };

        var res = await _client.PutAsJsonAsync($"/api/controles/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
           .GetProperty("Code")[0].GetString()
           .Should().Be("Code max length = 50.");

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT → 400 when Name > 100 chars")]
    public async Task Put_ShouldReturn400_WhenNameTooLong()
    {
        var id = Guid.NewGuid();
        var longName = new string('N', 101);

        var payload = new { ControleId = id, Code = "OK", Name = longName };

        var res = await _client.PutAsJsonAsync($"/api/controles/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
           .GetProperty("Name")[0].GetString()
           .Should().Be("Name max length = 100.");

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT → 404 when Controle not found")]
    public async Task Put_ShouldReturn404_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Controle?)null);

        var payload = new { ControleId = id, Code = "X", Name = "Y" };

        var res = await _client.PutAsJsonAsync($"/api/controles/{id}", payload);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT → 409 when duplicate Code exists on another row")]
    public async Task Put_ShouldReturn409_WhenDuplicateCode()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var target = Make(id1, code: "OLD");
        var duplicate = Make(id2, code: "DUP");

        _repo.Setup(r => r.GetByIdAsync(ControleId.Of(id1), It.IsAny<CancellationToken>()))
             .ReturnsAsync(target);
        _repo.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Controle, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync(duplicate);            

        var payload = new { ControleId = id1, Code = "DUP", Name = "Any" };

        var res = await _client.PutAsJsonAsync($"/api/controles/{id1}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT → 400 when ControleId empty GUID")]
    public async Task Put_ShouldReturn400_WhenIdEmpty()
    {
        var payload = new { ControleId = Guid.Empty, Code = "X", Name = "Y" };

        var res = await _client.PutAsJsonAsync($"/api/controles/{Guid.Empty}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT → 404 when route id malformed")]
    public async Task Put_ShouldReturn404_WhenRouteMalformed()
    {
        const string bad = "not-a-guid";
        var payload = new { ControleId = Guid.NewGuid(), Code = "X", Name = "Y" };

        var res = await _client.PutAsJsonAsync($"/api/controles/{bad}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}