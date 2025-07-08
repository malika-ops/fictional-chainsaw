using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ProductAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.ProductTests.CreateTests;

public class CreateProductEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "POST /api/products returns 201 and Guid (fixture version)")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Code = "001",
            Name = "Cash Express",
            CountryId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        // verify repository interaction using *FluentAssertions on Moq invocations
        _productRepoMock.Verify(r =>
            r.AddAsync(It.Is<Product>(r =>
                    r.Code == payload.Code &&
                    r.Name == payload.Name),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

    [Fact(DisplayName = "POST /api/products returns 400 & problem‑details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var invalidPayload = new
        {
            // Code intentionally omitted to trigger validation error
            Name = "Cash Express",
            CountryId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("Code")[0].GetString()
            .Should().Be("Code is required");

        // the handler must NOT be reached
        _productRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/products returns 400 & problem‑details when Name and Code are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndCodeAreMissing()
    {
        // ---------- Arrange ----------
        var invalidPayload = new
        {
        };

        // ---------- Act ----------
        var response = await _client.PostAsJsonAsync("/api/products", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // ---------- Assert ----------
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;

        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        var errors = root.GetProperty("errors");

        errors.GetProperty("Name")[0].GetString()
              .Should().Be("Name is required");

        errors.GetProperty("Code")[0].GetString()
              .Should().Be("Code is required");

        // handler must NOT run on validation failure
        _productRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/products returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "001";

        // Tell the repo mock that the code already exists
        var product = Product.Create(
            ProductId.Of(Guid.NewGuid()),
            duplicateCode,
            "Cash Express",
            true);

        _productRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var payload = new
        {
            Code = duplicateCode,
            Name = "Floussy",
            CountryId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetProperty("message").GetString();

        error.Should().Be($"Product with code : {duplicateCode} already exist");

        // Handler must NOT attempt to add the entity
        _productRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }
}
