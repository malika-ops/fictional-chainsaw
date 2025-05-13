using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.CreateTests;

public class CreateIdentityDocumentEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IIdentityDocumentRepository> _repoMock = new();

    public CreateIdentityDocumentEndpointTests(WebApplicationFactory<Program> factory)
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
                    .Setup(r => r.AddAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IdentityDocument e, CancellationToken _) => e);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }


    [Fact(DisplayName = "POST /api/identitydocuments returns 201 and Guid")]
    public async Task Post_ShouldReturn201_AndId_WhenRequestIsValid()
    {
        var payload = new
        {
            Code = "CIN",
            Name = "Carte Nationale",
            Description = "Document officiel"
        };

        var response = await _client.PostAsJsonAsync("/api/identitydocuments", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<IdentityDocument>(d =>
                d.Code == payload.Code && d.Name == payload.Name && d.Description == payload.Description),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/identitydocuments returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeMissing()
    {
        var invalidPayload = new
        {
            Name = "Passport",
            Description = "Missing code"
        };

        var response = await _client.PostAsJsonAsync("/api/identitydocuments", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/identitydocuments returns 400 when Name and Code are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndCodeMissing()
    {
        var payload = new { };

        var response = await _client.PostAsJsonAsync("/api/identitydocuments", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errors = doc!.RootElement.GetProperty("errors");
        errors.GetProperty("name")[0].GetString().Should().Be("Name is required");
        errors.GetProperty("code")[0].GetString().Should().Be("Code is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/identitydocuments returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        const string duplicateCode = "CIN";

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(Guid.NewGuid()),
            duplicateCode,
            "Existing Doc",
            null,
            true
        );

        _repoMock.Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(doc);

        var payload = new
        {
            Code = duplicateCode,
            Name = "Nouvelle Carte",
            Description = "Doublon"
        };

        var response = await _client.PostAsJsonAsync("/api/identitydocuments", payload);
        var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result!.RootElement.GetProperty("errors").GetString()
            .Should().Contain($"IdentityDocument with code : {duplicateCode} already exists");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
