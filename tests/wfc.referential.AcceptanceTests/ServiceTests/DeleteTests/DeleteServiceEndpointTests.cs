using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using AutoFixture;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Constants;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.DeleteTests;

public class DeleteServiceEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
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

        _serviceRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Setup TaxRuleDetailRepository to return empty collection (no dependencies)
        _taxRuleDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaxRuleDetail>());

        _serviceRepoMock.Setup(r => r.Update(It.IsAny<Service>()));

        // Act
        var response = await _client.DeleteAsync($"/api/services/{serviceId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify that the service was updated to inactive
        _serviceRepoMock.Verify(r => r.Update(It.Is<Service>(s => s.Id == ServiceId.Of(serviceId) && s.IsEnabled == false)), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.Service.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/services/{id} returns 404 when Service does not exist")]
    public async Task Delete_ShouldReturn404_WhenServiceDoesNotExist()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _serviceRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service)null);

        // Act
        var response = await _client.DeleteAsync($"/api/services/{serviceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify that Update was never called
        _serviceRepoMock.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
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

        _serviceRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var taxRuleDetail = _fixture.Create<TaxRuleDetail>();

        _taxRuleDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaxRuleDetail> { taxRuleDetail });

        // Act
        var response = await _client.DeleteAsync($"/api/services/{serviceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify that Update was never called
        _serviceRepoMock.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }
}