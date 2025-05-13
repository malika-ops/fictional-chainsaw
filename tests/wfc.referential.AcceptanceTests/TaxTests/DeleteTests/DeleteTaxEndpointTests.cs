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
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.TaxAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxTests.DeleteTests;

public class DeleteTaxEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRepository> _repoMock = new();
    private const string BaseUrl = "api/taxes";

    public DeleteTaxEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ITaxRepository>();
                services.RemoveAll<ICacheService>();

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
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
            "aa", "description", 43, 20);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == taxId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tax);

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.UpdateTaxAsync(It.Is<Tax>(r => r.Id == TaxId.Of(taxId) && !r.IsEnabled), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns 404 when tax does not exist")]
    public async Task Delete_ShouldReturn404_WhenTaxDoesNotExist()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(taxId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tax)null); // Tax not found

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
