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
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.ServiceControleTests.DeleteTests;

public class DeleteServiceControleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IServiceControleRepository> _repo = new();

    public DeleteServiceControleEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IServiceControleRepository>();
                s.RemoveAll<ICacheService>();

                _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }


    private static ServiceControle Make(Guid id,
                                        Guid svcId,
                                        Guid ctlId,
                                        Guid chnId,
                                        bool enabled = true)
    {
        var link = ServiceControle.Create(
            ServiceControleId.Of(id),
            ServiceId.Of(svcId),
            ControleId.Of(ctlId),
            ParamTypeId.Of(chnId),
            execOrder: 0);

        if (!enabled) link.Disable();
        return link;
    }


    [Fact(DisplayName = "DELETE /api/serviceControles/{id} → 200 when link exists")]
    public async Task Delete_ShouldReturn200_WhenLinkExists()
    {
        var id = Guid.NewGuid();
        var link = Make(id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _repo.Setup(r => r.GetByIdAsync(ServiceControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(link);

        ServiceControle? captured = null;
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
             .Callback(() => captured = link)
             .Returns(Task.CompletedTask);

        var resp = await _client.DeleteAsync($"/api/serviceControles/{id}");
        var ok = await resp.Content.ReadFromJsonAsync<bool>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        captured!.IsEnabled.Should().BeFalse();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "DELETE /api/serviceControles/{id} → 404 when link missing")]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        var missing = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(ServiceControleId.Of(missing), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ServiceControle?)null);

        var resp = await _client.DeleteAsync($"/api/serviceControles/{missing}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        doc!.RootElement.GetProperty("title").GetString().Should().Be("Resource Not Found");
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }



    [Fact(DisplayName = "DELETE /api/serviceControles/{id} → 400 when id is empty Guid")]
    public async Task Delete_ShouldReturn400_WhenIdEmpty()
    {
        var empty = Guid.Empty;

        var resp = await _client.DeleteAsync($"/api/serviceControles/{empty}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("ServiceControleId")[0].GetString()
            .Should().Be("ServiceControleId must be a non-empty GUID.");

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<ServiceControleId>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "DELETE /api/serviceControles/{id} → 404 when id malformed")]
    public async Task Delete_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var resp = await _client.DeleteAsync($"/api/serviceControles/{bad}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}