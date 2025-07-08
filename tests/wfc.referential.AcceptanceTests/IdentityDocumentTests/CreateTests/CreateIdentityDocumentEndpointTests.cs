using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.CreateTests;

public class CreateIdentityDocumentEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "POST /api/identitydocuments returns 200 and Guid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        _identityDocumentRepoMock.Setup(r => r.AddAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityDocument e, CancellationToken _) => e);

        _identityDocumentRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var payload = new
        {
            Code = "CIN",
            Name = "Carte Nationale",
            Description = "Document officiel"
        };

        var response = await _client.PostAsJsonAsync("/api/identitydocuments", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _identityDocumentRepoMock.Verify(r =>
            r.AddAsync(It.Is<IdentityDocument>(d =>
                d.Code == payload.Code && d.Name == payload.Name && d.Description == payload.Description),
                It.IsAny<CancellationToken>()), Times.Once);

        _identityDocumentRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            .GetProperty("Code")[0].GetString()
            .Should().Be("Identity document code is required.");

        _identityDocumentRepoMock.Verify(r => r.AddAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/identitydocuments returns 400 when Name and Code are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndCodeMissing()
    {
        var payload = new { };

        var response = await _client.PostAsJsonAsync("/api/identitydocuments", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errors = doc!.RootElement.GetProperty("errors");
        errors.GetProperty("Name")[0].GetString().Should().Be("Identity document name is required.");
        errors.GetProperty("Code")[0].GetString().Should().Be("Identity document code is required.");

        _identityDocumentRepoMock.Verify(r => r.AddAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/identitydocuments returns 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenCodeAlreadyExists()
    {
        const string duplicateCode = "CIN";

        var existingDoc = IdentityDocument.Create(
            IdentityDocumentId.Of(Guid.NewGuid()),
            duplicateCode,
            "Existing Doc",
            null
        );

        _identityDocumentRepoMock.Setup(r => r.GetOneByConditionAsync(
                It.Is<System.Linq.Expressions.Expression<System.Func<IdentityDocument, bool>>>(
                    expr => expr.ToString().Contains("Code")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDoc);

        var payload = new
        {
            Code = duplicateCode,
            Name = "Nouvelle Carte",
            Description = "Doublon"
        };

        var response = await _client.PostAsJsonAsync("/api/identitydocuments", payload);
        var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result!.RootElement.GetProperty("errors")
                .GetProperty("message").GetString()
            .Should().Contain($"Identity document with code {duplicateCode} already exists");

        _identityDocumentRepoMock.Verify(r => r.AddAsync(It.IsAny<IdentityDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}