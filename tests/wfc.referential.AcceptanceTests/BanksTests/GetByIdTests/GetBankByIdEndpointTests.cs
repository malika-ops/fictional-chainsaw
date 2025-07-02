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
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.BanksTests.GetByIdTests;

public class GetBankByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repo = new();

    public GetBankByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IBankRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Bank Make(Guid id, string code = "BANK-001", string? name = null, bool enabled = true)
    {
        var bank = Bank.Create(
            id: BankId.Of(id),
            code: code,
            name: name ?? $"Bank-{code}",
            abbreviation: code.Substring(0, Math.Min(3, code.Length)) // Default abbreviation from code
        );

        if (!enabled)
            bank.Disable();

        return bank;
    }

    private record BankDto(Guid Id, string Code, string Name, bool IsEnabled);

   
    [Fact(DisplayName = "GET /api/banks/{id} → 404 when Bank not found")]
    public async Task Get_ShouldReturn404_WhenBankNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(BankId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Bank?)null);

        var res = await _client.GetAsync($"/api/banks/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(BankId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/banks/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/banks/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<BankId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/banks/{id} → 200 for disabled Bank")]
    public async Task Get_ShouldReturn200_WhenBankDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "BANK-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(BankId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/banks/{id}");
        var dto = await res.Content.ReadFromJsonAsync<BankDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}