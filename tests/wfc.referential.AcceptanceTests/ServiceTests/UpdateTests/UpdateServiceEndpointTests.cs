using System.Linq.Expressions;
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
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.UpdateTests;

public class UpdateServiceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IServiceRepository> _repoMock = new();

    public UpdateServiceEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IServiceRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.Update(It.IsAny<Service>()));                    

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static Service DummyService(Guid id, string code, string name) =>
        Service.Create(ServiceId.Of(id), code, name, true, ProductId.Of(Guid.NewGuid()));

    [Fact(DisplayName = "PUT /api/services/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        var id = Guid.NewGuid();
        var oldService = DummyService(id, "SVC001", "ExpressService");

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync((Expression<Func<Service, bool>> predicate, CancellationToken _) =>
           {
               var func = predicate.Compile();

               if (func(oldService))
                   return oldService;

               return null;
           });

        Service? updated = null;
        _repoMock.Setup(r => r.Update(oldService))
                .Callback<Service>((rg) => updated = rg);

        var payload = new
        {
            Code = "NEW001",
            Name = "Updated Express",
            IsEnabled = true,
            ProductId = Guid.NewGuid()
        };

        var response = await _client.PutAsJsonAsync($"/api/services/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("NEW001");
        updated.Name.Should().Be("Updated Express");

        _repoMock.Verify(r => r.Update(It.IsAny<Service>()),
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
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required");

        _repoMock.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/services/{id} returns 400 when Code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        var id = Guid.NewGuid();
        var existing = DummyService(Guid.NewGuid(), "SVC001", "Express");
        var target = DummyService(id, "SVC002", "Transfer");

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
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

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Contain("Service with code : SVC001 already exists");

        _repoMock.Verify(r => r.Update(It.IsAny<Service>()), Times.Never);
    }
}
