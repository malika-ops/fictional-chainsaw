using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ProductAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.GetByIdTests;

public class GetServiceByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IServiceRepository> _repo = new();

    public GetServiceByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IServiceRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Service Make(Guid id, string code = "SERVICE-001", string? name = null, bool enabled = true)
    {
        var service = Service.Create(
            id: ServiceId.Of(id),
            code: code,
            name: name ?? $"Service-{code}",
            isEnabled: enabled,
            productId: ProductId.Of(Guid.NewGuid())
        );

        if (!enabled)
            service.SetInactive();

        return service;
    }

    public class StringToBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                return bool.TryParse(stringValue, out var result) && result;
            }
            return reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }

    private record ServiceDto(
        Guid Id,
        string Code,
        string Name,
        [property: JsonConverter(typeof(StringToBooleanConverter))] bool IsEnabled
    );

    [Fact(DisplayName = "GET /api/services/{id} → 404 when Service not found")]
    public async Task Get_ShouldReturn404_WhenServiceNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(ServiceId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Service?)null);

        var res = await _client.GetAsync($"/api/services/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(ServiceId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/services/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/services/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/services/{id} → 200 for disabled Service")]
    public async Task Get_ShouldReturn200_WhenServiceDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "SERVICE-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(ServiceId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/services/{id}");
        var dto = await res.Content.ReadFromJsonAsync<ServiceDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}