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
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.PatchTests;

public class PatchIdentityDocumentEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IIdentityDocumentRepository> _repoMock = new();

    public PatchIdentityDocumentEndpointTests(WebApplicationFactory<Program> factory)
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

    [Fact(DisplayName = "PATCH /api/identitydocuments/{id} returns updated Guid")]
    public async Task Patch_ShouldReturnUpdatedId_WhenValid()
    {
        var docId = Guid.NewGuid();
        var existing = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte Nationale",
            "Original",
            true);

        _repoMock.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var patch = new PatchIdentityDocumentRequest
        {
            IdentityDocumentId = docId,
            Code = "CIN-UPD",
            Name = "Carte Nationale Modifiée",
            Description = "Mise à jour",
            IsEnabled = false
        };

        var response = await _client.PatchAsync($"/api/identitydocuments/{docId}", JsonContent.Create(patch));
        var updatedId = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedId.Should().Be(docId);
        existing.Name.Should().Be(patch.Name);
        existing.Code.Should().Be(patch.Code);
        existing.Description.Should().Be(patch.Description);
    }

    [Fact(DisplayName = "PATCH /api/identitydocuments/{id} returns 404 if not found")]
    public async Task Patch_ShouldReturn404_WhenNotFound()
    {
        var docId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityDocument?)null);

        var patch = new PatchIdentityDocumentRequest
        {
            IdentityDocumentId = docId,
            Code = "X",
            Name = "Test"
        };

        var response = await _client.PatchAsync($"/api/identitydocuments/{docId}", JsonContent.Create(patch));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PATCH /api/identitydocuments/{id} returns 400 when validation fails")]
    public async Task Patch_ShouldReturn400_WhenInvalid()
    {
        var docId = Guid.NewGuid();
        var existing = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte Nationale",
            null,
            true);

        _repoMock.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var patch = new PatchIdentityDocumentRequest
        {
            IdentityDocumentId = docId,
            Code = "",
            Name = "OK"
        };

        var response = await _client.PatchAsync($"/api/identitydocuments/{docId}", JsonContent.Create(patch));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
