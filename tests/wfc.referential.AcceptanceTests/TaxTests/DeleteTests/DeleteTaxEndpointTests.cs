using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxTests.DeleteTests;

public class DeleteTaxEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRepository> _repoMock = new();
    private readonly Mock<ITaxRuleDetailRepository> _taxRuleDetailRepositoryMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private const string BaseUrl = "api/taxes";

    public DeleteTaxEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Clone the factory and customize the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ITaxRepository>();
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

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns true when tax is deleted successfully")]
    public async Task Delete_ShouldReturnTrue_WhenTaxExists()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        var tax = Tax.Create(
            TaxId.Of(taxId),
            "01",
            "testAAB",
            "aa",
            "description",
            43,
            20);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Tax, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tax);

        _taxRuleDetailRepositoryMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaxRuleDetail>());

        _repoMock.Setup(r => r.Update(It.IsAny<Tax>()));
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.Is<Tax>(t => t.IsEnabled == false)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.Tax.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns 404 when tax does not exist")]
    public async Task Delete_ShouldReturn404_WhenTaxDoesNotExist()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Tax, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tax)null); // Tax not found

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.Update(It.IsAny<Tax>()), Times.Never);
    }

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns 400 when tax has tax rule details")]
    public async Task Delete_ShouldReturn400_WhenTaxHasTaxRuleDetails()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        var tax = Tax.Create(
            TaxId.Of(taxId),
            "01",
            "testAAB",
            "aa",
            "description",
            43,
            20);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Tax, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tax);

        var taxRuleDetail = (TaxRuleDetail)FormatterServices.GetUninitializedObject(typeof(TaxRuleDetail));

        _taxRuleDetailRepositoryMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaxRuleDetail> { taxRuleDetail });

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify that Update was never called
        _repoMock.Verify(r => r.Update(It.IsAny<Tax>()), Times.Never);
    }
}