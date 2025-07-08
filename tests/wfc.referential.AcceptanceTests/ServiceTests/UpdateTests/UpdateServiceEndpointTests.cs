using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.UpdateTests;

public class UpdateServiceEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Service DummyService(Guid id, string code, string name) =>
        Service.Create(ServiceId.Of(id), code, name, true, ProductId.Of(Guid.NewGuid()));

    [Fact(DisplayName = "PUT /api/services/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        var id = Guid.NewGuid();
        var oldService = DummyService(id, "SVC001", "ExpressService");


        _serviceRepoMock.SetupSequence(r => r.GetOneByConditionAsync(
        It.IsAny<Expression<Func<Service, bool>>>(),
        It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Service?>(oldService))  // Premier appel
            .Returns(Task.FromResult<Service?>(null));
        Service? updated = null;
        _serviceRepoMock.Setup(r => r.Update(oldService))
                .Callback<Service>((rg) => updated = rg);

        var payload = new
        {
            Code = "NEW001",
            Name = "Updated Express",
            IsEnabled = true,
            ProductId = Guid.NewGuid()
        };

        var response = await _client.PutAsJsonAsync($"/api/services/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(true);

        updated!.Code.Should().Be("NEW001");
        updated.Name.Should().Be("Updated Express");

        _serviceRepoMock.Verify(r => r.Update(It.IsAny<Service>()),
                         Times.Once);
    }

    [Fact(DisplayName = "PUT /api/services/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        var id = Guid.NewGuid();
        var payload = new
        {
            Code = "NEW001",
            IsEnabled = true,
            ProductId = Guid.NewGuid()
        };

        var response = await _client.PutAsJsonAsync($"/api/services/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("Name")[0].GetString()
            .Should().Be("Name is required");

        _serviceRepoMock.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/services/{id} returns 400 when Code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        var id = Guid.NewGuid();
        var existing = DummyService(Guid.NewGuid(), "SVC001", "Express");
        var target = DummyService(id, "SVC002", "Transfer");

        _serviceRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _serviceRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing); // duplicate code

        var payload = new
        {
            ServiceId = id,
            Code = "SVC001",
            Name = "Express",
            IsEnabled = true,
            ProductId = Guid.NewGuid()
        };

        var response = await _client.PutAsJsonAsync($"/api/services/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetProperty("message").GetString()
           .Should().Contain("Service with code : SVC001 already exists");

        _serviceRepoMock.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }
}
