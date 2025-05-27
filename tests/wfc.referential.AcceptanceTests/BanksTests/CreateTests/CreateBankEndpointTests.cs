using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.BanksTests.CreateTests;

public class CreateBankAcceptanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public CreateBankAcceptanceTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBankRepository>();

                _repoMock.Setup(r => r.AddAsync(It.IsAny<Bank>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Bank b, CancellationToken _) => b);
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Bank)null);

                services.AddSingleton(_repoMock.Object);
            });
        });
        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/banks creates bank with all required fields")]
    public async Task CreateBank_Should_CreateNewBank_WhenAllRequiredFieldsProvided()
    {
        // Arrange
        var createRequest = new CreateBankRequest
        {
            Code = "AWB",
            Name = "Attijariwafa Bank",
            Abbreviation = "AWB"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/banks", createRequest);
        var bankId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        bankId.Should().NotBeEmpty();

        _repoMock.Verify(r => r.AddAsync(It.Is<Bank>(b =>
            b.Code == "AWB" &&
            b.Name == "Attijariwafa Bank" &&
            b.Abbreviation == "AWB" &&
            b.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/banks returns 400 when Code is empty")]
    public async Task CreateBank_Should_ReturnValidationError_WhenBankCodeIsEmpty()
    {
        // Arrange
        var invalidRequest = new CreateBankRequest
        {
            Code = "",
            Name = "Test Bank",
            Abbreviation = "TB"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/banks", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Bank>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/banks returns 409 when duplicate Code is provided")]
    public async Task CreateBank_Should_ReturnConflictError_WhenBankCodeAlreadyExists()
    {
        // Arrange
        var existingBank = Bank.Create(
            BankId.Of(Guid.NewGuid()),
            "AWB", "Attijariwafa Bank", "AWB");

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBank);

        var duplicateRequest = new CreateBankRequest
        {
            Code = "AWB",
            Name = "Another Bank",
            Abbreviation = "AB"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/banks", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Bank>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/banks auto-generates bank ID")]
    public async Task CreateBank_Should_AutoGenerateBankId_WhenBankIsCreated()
    {
        // Arrange
        var createRequest = new CreateBankRequest
        {
            Code = "BMCE",
            Name = "Banque Marocaine du Commerce Extérieur",
            Abbreviation = "BMCE"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/banks", createRequest);
        var bankId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        bankId.Should().NotBeEmpty();

        _repoMock.Verify(r => r.AddAsync(It.Is<Bank>(b =>
            b.Id != null && b.Id.Value != Guid.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/banks sets IsEnabled to true by default")]
    public async Task CreateBank_Should_SetIsEnabledToTrue_ByDefault()
    {
        // Arrange
        var createRequest = new CreateBankRequest
        {
            Code = "SG",
            Name = "Société Générale Maroc",
            Abbreviation = "SG"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/banks", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _repoMock.Verify(r => r.AddAsync(It.Is<Bank>(b =>
            b.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = "POST /api/banks validates all required fields")]
    [InlineData("", "Name", "Abbreviation")]
    [InlineData("CODE", "", "Abbreviation")]
    [InlineData("CODE", "Name", "")]
    public async Task CreateBank_Should_ReturnValidationError_WhenRequiredFieldsAreMissing(
        string code, string name, string abbreviation)
    {
        // Arrange
        var invalidRequest = new CreateBankRequest
        {
            Code = code,
            Name = name,
            Abbreviation = abbreviation
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/banks", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Bank>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}