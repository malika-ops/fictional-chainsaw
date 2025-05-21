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
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.UpdateTests;

public class UpdateIdentityDocumentEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IIdentityDocumentRepository> _repoMock = new();

    public UpdateIdentityDocumentEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IIdentityDocumentRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static IdentityDocument Dummy(Guid id, string code, string name) =>
        IdentityDocument.Create(IdentityDocumentId.Of(id), code, name, "desc");

    [Fact(DisplayName = "PUT /api/identitydocuments/{id} updates successfully")]
    public async Task Put_ShouldUpdateSuccessfully()
    {
        var id = Guid.NewGuid();
        var original = Dummy(id, "CIN", "Carte Nationale");

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(original);

        var payload = new
        {
            Code = "CIN-UPD",
            Name = "Carte Modifiée",
            Description = "Nouveau texte",
            IsEnabled = false
        };

        var response = await _client.PutAsJsonAsync($"/api/identitydocuments/{id}", payload);
        var resultId = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        resultId.Should().Be(id);

        original.Code.Should().Be(payload.Code);
        original.Name.Should().Be(payload.Name);
        original.Description.Should().Be(payload.Description);
    }

    [Fact(DisplayName = "PUT /api/identitydocuments/{id} returns 400 if name missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        var id = Guid.NewGuid();
        var payload = new
        {
            Code = "CIN-TEST",
            Description = "desc",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/identitydocuments/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors").GetProperty("name")[0].GetString()
            .Should().Be("Name is required");
    }

    [Fact(DisplayName = "PUT /api/identitydocuments/{id} returns 404 if not found")]
    public async Task Put_ShouldReturn404_WhenNotFound()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IdentityDocument?)null);

        var payload = new
        {
            Code = "XX",
            Name = "Unknown",
            Description = "x",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/identitydocuments/{id}", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
