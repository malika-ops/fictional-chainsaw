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
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.PatchTests;

public class PatchServiceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IServiceRepository> _repoMock = new();

    public PatchServiceEndpointTests(WebApplicationFactory<Program> factory)
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

    [Fact(DisplayName = "PATCH /api/services/{id} updates the Service successfully")]
    public async Task PatchService_ShouldReturnUpdatedServiceId_WhenServiceExists()
    {
        var serviceId = Guid.NewGuid();
        var patchRequest = new PatchServiceRequest
        {
            ServiceId = serviceId,
            Code = "SVC-NEW",
            Name = "New Service Name",
            IsEnabled = true
        };

        var service = Service.Create(
            ServiceId.Of(serviceId),
            "SVC-OLD",
            "Old Name",
            true,
            ProductId.Of(Guid.NewGuid())
        );

        _repoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var response = await _client.PatchAsync($"/api/services/{serviceId}", JsonContent.Create(patchRequest));
        var updatedServiceId = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedServiceId.Should().Be(serviceId);
        service.Name.Should().BeEquivalentTo(patchRequest.Name);
    }

    [Fact(DisplayName = "PATCH /api/services/{id} returns 404 when Service does not exist")]
    public async Task PatchService_ShouldReturnNotFound_WhenServiceDoesNotExist()
    {
        var serviceId = Guid.NewGuid();
        var patchRequest = new PatchServiceRequest
        {
            ServiceId = serviceId,
            Code = "non-existing-code",
            Name = "Non-existing Service",
        };

        _repoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service)null);

        var response = await _client.PatchAsync($"/api/services/{serviceId}", JsonContent.Create(patchRequest));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PATCH /api/services/{id} returns 400 when validation fails")]
    public async Task PatchService_ShouldReturnBadRequest_WhenValidationFails()
    {
        var serviceId = Guid.NewGuid();
        var patchRequest = new PatchServiceRequest
        {
            ServiceId = serviceId,
            Code = "", // Invalid code
            Name = "Invalid Service",
        };

        _repoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Service.Create(
                ServiceId.Of(serviceId),
                "SVC-OLD",
                "Old Name",
                true,
                ProductId.Of(Guid.NewGuid())
            ));

        var response = await _client.PatchAsync($"/api/services/{serviceId}", JsonContent.Create(patchRequest));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
