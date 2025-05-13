using FluentValidation.TestHelper;
using wfc.referential.Application.Currencies.Commands.DeleteCurrency;
using Xunit;

namespace wfc.referential.UnitTests.Currencies.Application;

public class DeleteCurrencyCommandValidatorTests
{
    private readonly DeleteCurrencyCommandValidator _validator;

    public DeleteCurrencyCommandValidatorTests()
    {
        _validator = new DeleteCurrencyCommandValidator();
    }

    [Fact]
    public void Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new DeleteCurrencyCommand(Guid.NewGuid().ToString());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]                                  // Empty string
    [InlineData("invalid-guid")]                      // Not a GUID
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty GUID
    [InlineData(null)]                                // Null
    public void Validate_WithInvalidGuid_ShouldHaveErrors(string currencyId)
    {
        // Arrange
        var command = new DeleteCurrencyCommand(currencyId);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CurrencyId);
    }
}