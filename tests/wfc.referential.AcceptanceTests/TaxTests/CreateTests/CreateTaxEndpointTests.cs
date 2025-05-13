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
using wfc.referential.Application.Taxes.Dtos;
using wfc.referential.Domain.TaxAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.TaxTests.CreateTests;

public class CreateTaxEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cachMock = new();
    private const string BaseUrl = "api/taxes";

    public CreateTaxEndpointTests(WebApplicationFactory<Program> factory)
    {

        // clone the factory and customise the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ITaxRepository>();
                services.RemoveAll<ICacheService>();

                // 🪄  Set up mock behaviour (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddTaxAsync(It.IsAny<Tax>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Tax r, CancellationToken _) => r);

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cachMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = $"POST {BaseUrl} returns 200 and Guid (fixture version)")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new CreateTaxRequest
        {
            Code = "TX001",
            CodeEn = "VAT",
            CodeAr = "ضريبة القيمة المضافة",
            Description = "Value Added Tax applicable to goods and services",
            FixedAmount = 25,
            Value = 15
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        //verify repository interaction using * FluentAssertions on Moq invocations
        _repoMock.Verify(r =>
            r.AddTaxAsync(It.Is<Tax>(r =>
                    r.Code == payload.Code &&
                    r.CodeEn == payload.CodeEn &&
                    r.CodeAr == payload.CodeAr &&
                    r.Description == payload.Description &&
                    r.FixedAmount == payload.FixedAmount &&
                    r.Rate == payload.Value),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

    [Fact(DisplayName = $"POST {BaseUrl} returns 400 & problem‑details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var invalidPayload = new
        {
            Code = "TX001",
            CodeAr = "ضريبة القيمة المضافة",
            Description = "Value Added Tax applicable to goods and services",
            FixedAmount = 243,
            Value = 15,
            EffectiveDate = "2025-01-01T00:00:00Z",
            ExpiryDate = "2030-12-31T23:59:59Z"
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("codeEn")[0].GetString()
            .Should().Be("CodeEn Code is required");

        // the handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddTaxAsync(It.IsAny<Tax>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = $"POST {BaseUrl} returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "99";

        // Tell the repo mock that the code already exists
        var tax = Tax.Create(
            TaxId.Create(),
            duplicateCode,"codeEn", "codeAr", "description",42,20);

        _repoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tax);

        var payload = new CreateTaxRequest
        {
            Code = duplicateCode,
            CodeEn = "VAT",
            CodeAr = "ضريبة القيمة المضافة",
            Description = "Value Added Tax applicable to goods and services",
            FixedAmount = 32,
            Value = 15
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"{nameof(Tax)} with code : {duplicateCode} already exist");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddTaxAsync(It.IsAny<Tax>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }

}
