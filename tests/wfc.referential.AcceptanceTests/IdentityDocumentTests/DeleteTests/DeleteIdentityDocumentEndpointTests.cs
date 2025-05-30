using System.Net;
using System.Net.Http.Json;
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
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IIdentityDocumentRepository>();

                services.AddSingleton(_repoMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/identitydocuments/{id} returns true when document exists")]
    public async Task Delete_ShouldReturnTrue_WhenDocumentExists()
    {
        var docId = Guid.NewGuid();
        var identityDocumentId = IdentityDocumentId.Of(docId);
        var entity = IdentityDocument.Create(
            identityDocumentId,
            "CIN",
            "Carte Nationale",
            "Valid doc"
        );

        _repoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var response = await _client.DeleteAsync($"/api/identitydocuments/{docId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify that Disable() was called (entity should not be enabled)
        entity.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/identitydocuments/{id} returns 404 when document does not exist")]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        var docId = Guid.NewGuid(); // Use a valid GUID, not Empty
        var identityDocumentId = IdentityDocumentId.Of(docId);

        _repoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityDocument?)null);

        var response = await _client.DeleteAsync($"/api/identitydocuments/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/identitydocuments/{id} returns 400 when ID is invalid")]
    public async Task Delete_ShouldReturn400_WhenIdIsInvalid()
    {
        var response = await _client.DeleteAsync("/api/identitydocuments/invalid-guid");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}