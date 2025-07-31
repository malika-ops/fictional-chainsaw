using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.CurrencyDenominations.Dtos;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyDenominationTests.CreateTests;

public class CreateCurrencyDenominationAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "POST /api/currencyDenominations creates currency denomination with all required fields")]
    public async Task CreateCurrencyDenomination_Should_CreateNewCurrencyDenomination_WhenAllRequiredFieldsProvided()
    {
        // Arrange
        var createRequest = new CreateCurrencyDenominationRequest
        {
            CurrencyId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1"),
            Type = CurrencyDenominationType.Coin,
            Value = 100
        };

        var currency = Currency.Create(CurrencyId.Of(createRequest.CurrencyId), "MAD","","", "Moroccan Dirham", 10);

        _currencyRepoMock
            .Setup(r => r.GetByIdAsync(CurrencyId.Of(createRequest.CurrencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _currencyDenominationRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<CurrencyDenomination, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var json = JsonSerializer.Serialize(createRequest);
        Console.WriteLine(">>> JSON Sent:\n" + json);

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencyDenominations", createRequest);

        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(">>>>> RESPONSE BODY:\n" + body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var currencyDenominationId = await response.Content.ReadFromJsonAsync<Guid>();
        currencyDenominationId.Should().NotBeEmpty();

        _currencyDenominationRepoMock.Verify(r => r.AddAsync(It.Is<CurrencyDenomination>(c =>
            c.CurrencyId == CurrencyId.Of(createRequest.CurrencyId) &&
            c.Type == createRequest.Type &&
            c.Value == createRequest.Value &&
            c.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);

        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/currencyDenominations sets IsEnabled to true by default")]
    public async Task CreateCurrencyDenomination_Should_SetIsEnabledToTrue_ByDefault()
    {
        // Arrange
        var createRequest = new CreateCurrencyDenominationRequest
        {
            CurrencyId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1"),
            Type = CurrencyDenominationType.Banknote,
            Value = 500
        };

        var currency = Currency.Create(CurrencyId.Of(createRequest.CurrencyId), "MAD", "", "", "Moroccan Dirham", 10);

        _currencyRepoMock
            .Setup(r => r.GetByIdAsync(CurrencyId.Of(createRequest.CurrencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _currencyDenominationRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<CurrencyDenomination, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencyDenominations", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _currencyDenominationRepoMock.Verify(r => r.AddAsync(It.Is<CurrencyDenomination>(c =>
            c.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);
    }
}
