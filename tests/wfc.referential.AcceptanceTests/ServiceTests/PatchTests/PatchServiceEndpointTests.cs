using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.PatchTests;

public class PatchServiceEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PATCH /api/services/{id} updates the Service successfully")]
    public async Task PatchService_ShouldReturnUpdatedServiceId_WhenServiceExists()
    {
        var serviceId = Guid.NewGuid();
        var patchRequest = new PatchServiceRequest
        {
            Code = "SVC-NEW",
            Name = "New Service Name",
            IsEnabled = true
        };

        var service = Service.Create(
            ServiceId.Of(serviceId),
            "SVC-OLD",
            "Old Name",
            FlowDirection.None,
            true,
            ProductId.Of(Guid.NewGuid())
        );

        _serviceRepoMock.SetupSequence(r => r.GetOneByConditionAsync(
            It.IsAny<Expression<Func<Service, bool>>>(),
            It.IsAny<CancellationToken>()))
        .Returns(Task.FromResult<Service?>(service)) 
        .Returns(Task.FromResult<Service?>(null));

        var response = await _client.PatchAsync($"/api/services/{serviceId}", JsonContent.Create(patchRequest));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(true);
        service.Name.Should().BeEquivalentTo(patchRequest.Name);
    }

    [Fact(DisplayName = "PATCH /api/services/{id} returns 404 when Service does not exist")]
    public async Task PatchService_ShouldReturnNotFound_WhenServiceDoesNotExist()
    {
        var serviceId = Guid.NewGuid();
        var patchRequest = new PatchServiceRequest
        {
            Code = "non-existing-code",
            Name = "Non-existing Service",
        };

        _serviceRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
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
            Code = "", // Invalid code
            Name = "Invalid Service",
        };

        _serviceRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Service.Create(ServiceId.Of(serviceId), "code", "name", FlowDirection.Debit, true,ProductId.Of(Guid.NewGuid())));

        var response = await _client.PatchAsync($"/api/services/{serviceId}", JsonContent.Create(patchRequest));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
