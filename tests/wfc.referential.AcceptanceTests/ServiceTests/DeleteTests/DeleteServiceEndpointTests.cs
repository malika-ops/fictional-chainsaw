using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.DeleteTests;

public class DeleteServiceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IServiceRepository> _repoMock = new();
    private readonly Mock<ITaxRuleDetailRepository> _taxRuleDetailRepositoryMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    public DeleteServiceEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IServiceRepository>();
                services.RemoveAll<ITaxRuleDetailRepository>();
                services.RemoveAll<ICacheService>();

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_taxRuleDetailRepositoryMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/services/{id} returns true when Service is deleted successfully")]
    public async Task Delete_ShouldReturnTrue_WhenServiceExists()
    {
        // Arrange
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

        // Setup TaxRuleDetailRepository to return empty collection (no dependencies)
        _taxRuleDetailRepositoryMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaxRuleDetail>());

        _repoMock.Setup(r => r.Update(It.IsAny<Service>()));

        // Act
        var response = await _client.DeleteAsync($"/api/services/{serviceId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify that the service was updated to inactive
        _repoMock.Verify(r => r.Update(It.Is<Service>(s => s.Id == ServiceId.Of(serviceId) && s.IsEnabled == false)), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.Service.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/services/{id} returns 404 when Service does not exist")]
    public async Task Delete_ShouldReturn404_WhenServiceDoesNotExist()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service)null);

        // Act
        var response = await _client.DeleteAsync($"/api/services/{serviceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify that Update was never called
        _repoMock.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/services/{id} returns 400 when Service has tax rule details")]
    public async Task Delete_ShouldReturn400_WhenServiceHasTaxRuleDetails()
    {
        // Arrange
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

        var taxRuleDetail = (TaxRuleDetail)FormatterServices.GetUninitializedObject(typeof(TaxRuleDetail));

        _taxRuleDetailRepositoryMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaxRuleDetail> { taxRuleDetail });

        // Act
        var response = await _client.DeleteAsync($"/api/services/{serviceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify that Update was never called
        _repoMock.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }
}