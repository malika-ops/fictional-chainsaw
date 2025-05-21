using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.DeleteTests;

public class DeleteIdentityDocumentEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IIdentityDocumentRepository> _repoMock = new();

    public DeleteIdentityDocumentEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IIdentityDocumentRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/identitydocuments/{id} returns true when document exists")]
    public async Task Delete_ShouldReturnTrue_WhenDocumentExists()
    {
        var docId = Guid.NewGuid();
        var entity = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte Nationale",
            "Valid doc"
            );

        _repoMock.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var response = await _client.DeleteAsync($"/api/identitydocuments/{docId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r =>
            r.UpdateAsync(It.Is<IdentityDocument>(d => d.Id == IdentityDocumentId.Of(docId) && !d.IsEnabled),
                          It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/identitydocuments/{id} returns 404 when document does not exist")]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        var docId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityDocument?)null);

        var response = await _client.DeleteAsync($"/api/identitydocuments/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
