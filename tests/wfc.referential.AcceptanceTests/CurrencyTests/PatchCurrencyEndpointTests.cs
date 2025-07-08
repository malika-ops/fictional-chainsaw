using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.PatchTests;

public class PatchCurrencyAcceptanceTests (TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "PATCH /api/currencies/{id} modifies only provided fields")]
    public async Task PatchCurrency_Should_ModifyOnlyProvidedFields_WhenPartialDataSent()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var originalCurrency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalCurrency);
        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var patchRequest = new PatchCurrencyRequest
        {
            Name = "Updated US Dollar Name", // Only updating name
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify only the name was changed, other fields remain unchanged
        originalCurrency.Name.Should().Be("Updated US Dollar Name");
        originalCurrency.Code.Should().Be("USD"); // Unchanged
        originalCurrency.CodeIso.Should().Be(840); // Unchanged

        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Once);
        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} validates duplicates for changed fields only")]
    public async Task PatchCurrency_Should_ValidateDuplicates_OnlyForChangedFields()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var existingCurrencyId = Guid.NewGuid();

        var targetCurrency = Currency.Create(CurrencyId.Of(currencyId), "USD", "دولار", "Dollar", "Dollar", 840);
        var conflictingCurrency = Currency.Create(CurrencyId.Of(existingCurrencyId), "EUR", "يورو", "Euro", "Euro", 978);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetCurrency);
        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingCurrency);

        var patchRequest = new PatchCurrencyRequest
        {
            Code = "EUR", // Attempting to change to existing code
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} allows same values in unchanged fields")]
    public async Task PatchCurrency_Should_AllowSameValues_WhenNotChangingFields()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);
        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var patchRequest = new PatchCurrencyRequest
        {
            Code = "USD", // Same code as current (should be allowed)
            Name = "New Name" // Different name
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Once);
        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} returns 400 when currency not found")]
    public async Task PatchCurrency_Should_ReturnBadRequest_WhenCurrencyNotFound()
    {
        // Arrange
        var currencyId = Guid.NewGuid();

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var patchRequest = new PatchCurrencyRequest
        {
            Name = "New Name"
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Never);
    }

    [Theory(DisplayName = "PATCH /api/currencies/{id} updates individual fields correctly")]
    [InlineData("NewCode", null, null, null, null, null)]
    [InlineData(null, "NewName", null, null, null, null)]
    [InlineData(null, null, "كود عربي جديد", null, null, null)]
    [InlineData(null, null, null, "New English Code", null, null)]
    [InlineData(null, null, null, null, 999, null)]
    [InlineData(null, null, null, null, null, false)]
    public async Task PatchCurrency_Should_UpdateIndividualFields_Correctly(
        string code, string name, string codeAR, string codeEN, int? codeIso, bool? isEnabled)
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD", "دولار", "Dollar", "Dollar", 840);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);
        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var patchRequest = new PatchCurrencyRequest
        {
            Code = code,
            Name = name,
            CodeAR = codeAR,
            CodeEN = codeEN,
            CodeIso = codeIso,
            IsEnabled = isEnabled
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Once);
        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}