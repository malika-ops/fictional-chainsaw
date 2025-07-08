using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.CreateTests;

public class CreateMonetaryZoneEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static object ValidPayload(string? code = null,
                                       string? name = null,
                                       string? description = null)
    {
        return new
        {
            Code = code ?? "EU",
            Name = name ?? "Europe",
            Description = description ?? "European monetary zone"
        };
    }


    [Fact(DisplayName = "POST /api/monetaryZones → 200 & Guid on valid request")]
    public async Task Post_ShouldReturn200_AndId_WhenValid()
    {
        // Arrange
        var payload = ValidPayload();
        var code = "EU"; var name = "Europe"; var desc = "European monetary zone";

        // Act
        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _monetaryZoneRepoMock.Verify(r =>
            r.AddAsync(It.Is<MonetaryZone>(m =>
                    m.Code == code &&
                    m.Name == name &&
                    m.Description == desc),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _producerServiceMock.Verify(p => p.ProduceAsync(It.IsAny<object>(), "auditLogsTopic"), Times.Once);
    }

    [Fact(DisplayName = "POST /api/monetaryZones → 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenDuplicateCode()
    {
        // Arrange
        const string dup = "EU";
        var existing = MonetaryZone.Create(
            MonetaryZoneId.Of(Guid.NewGuid()), dup, "Old-Europe", "Desc");

        _monetaryZoneRepoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", ValidPayload(code: dup));

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _monetaryZoneRepoMock.Verify(r => r.AddAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/monetaryZones → 400 when Code exceeds 20 chars")]
    public async Task Post_ShouldReturn400_WhenCodeTooLong()
    {
        var longCode = new string('X', 21);
        var payload = ValidPayload(code: longCode);

        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
           .GetProperty("Code")[0].GetString()
           .Should().Be("Code must be less than 10 characters");   

        _monetaryZoneRepoMock.Verify(r => r.AddAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/monetaryZones → 400 when Name missing")]
    public async Task Post_ShouldReturn400_WhenNameMissing()
    {
        var payload = new { Code = "EU" };   

        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
           .GetProperty("Name")[0].GetString()
           .Should().Be("Name is required");

        _monetaryZoneRepoMock.Verify(r => r.AddAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/monetaryZones → 200 when Description omitted")]
    public async Task Post_ShouldReturn200_WhenDescriptionOmitted()
    {
        // Arrange
        var code = "AF";
        var name = "Africa";
        var payload = new { Code = code, Name = name };  

        // Act
        var resp = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _monetaryZoneRepoMock.Verify(r =>
            r.AddAsync(It.Is<MonetaryZone>(m =>
                    m.Code == code &&
                    m.Name == name &&
                    m.Description == string.Empty),   
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}