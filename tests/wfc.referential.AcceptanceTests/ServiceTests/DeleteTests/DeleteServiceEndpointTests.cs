using System.Linq.Expressions;
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
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.DeleteTests;

public class DeleteServiceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IServiceRepository> _repoMock = new();

    public DeleteServiceEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IServiceRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/services/{id} returns true when Service is deleted successfully")]
    public async Task Delete_ShouldReturnTrue_WhenServiceExists()
    {
        var serviceId = Guid.NewGuid();
        var service = Service.Create(
            ServiceId.Of(serviceId),
            "SVC001",
            "ExpressService",
            true,
            ProductId.Of(Guid.NewGuid())
        );

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var response = await _client.DeleteAsync($"/api/services/{serviceId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.Is<Service>(r => r.Id == ServiceId.Of(serviceId) && !r.IsEnabled.Equals(true))), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/services/{id} returns 404 when Service does not exist")]
    public async Task Delete_ShouldReturn404_WhenServiceDoesNotExist()
    {
        var serviceId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service)null);

        var response = await _client.DeleteAsync($"/api/services/{serviceId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
