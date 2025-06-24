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
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.CreateTests;

public class CreateServiceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IServiceRepository> _repoMock = new();
    private readonly Mock<IProductRepository> _repöProductMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    public CreateServiceEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IServiceRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Service s, CancellationToken _) => s);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
                services.AddSingleton(_repöProductMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/services returns 201 and Guid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        var payload = new CreateServiceRequest
        {
            Code = "SVC001",
            Name = "Express Transfer",
            ProductId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        _repöProductMock.Setup(r => r.GetByIdAsync(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProductId productId, CancellationToken _) =>
                        Product.Create(productId, "TEST Code", "Test Name",true));

        var response = await _client.PostAsJsonAsync("/api/services", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<Service>(s =>
                    s.Code == payload.Code &&
                    s.Name == payload.Name),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/services returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        var invalidPayload = new
        {
            Name = "Express Transfer",
            ProductId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/services", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("Code")[0].GetString()
            .Should().Be("Code is required");

        _repoMock.Verify(r =>
            r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/services returns 400 when Name and Code are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndCodeAreMissing()
    {
        var invalidPayload = new
        {
            ProductId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/services", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var errors = root.GetProperty("errors");

        errors.GetProperty("Name")[0].GetString()
              .Should().Be("Name is required");

        errors.GetProperty("Code")[0].GetString()
              .Should().Be("Code is required");

        _repoMock.Verify(r =>
            r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/services returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        const string duplicateCode = "SVC001";

        var service = Service.Create(
            ServiceId.Of(Guid.NewGuid()),
            duplicateCode,
            "Express",
            true,
            ProductId.Of(Guid.NewGuid())
        );

        _repoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var payload = new
        {
            Code = duplicateCode,
            Name = "Fast Service",
            ProductId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/services", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetProperty("message").GetString();

        error.Should().Contain($"Service with code : {duplicateCode} already exists");

        _repoMock.Verify(r =>
            r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
