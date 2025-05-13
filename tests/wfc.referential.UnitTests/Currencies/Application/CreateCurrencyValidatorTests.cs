using FluentValidation.TestHelper;
using wfc.referential.Application.Currencies.Commands.CreateCurrency;
using Xunit;

namespace wfc.referential.UnitTests.Currencies.Application;

public class CreateCurrencyValidatorTests
{
    private readonly CreateCurrencyValidator _validator;

    public CreateCurrencyValidatorTests()
    {
        _validator = new CreateCurrencyValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateCurrencyCommand("USD", "US Dollar", "دولار أمريكي", "US Dollar", 840);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "US Dollar", 840)]    // Empty Code
    [InlineData(null, "US Dollar", 840)]  // Null Code
    [InlineData("USD", "", 840)]          // Empty Name
    [InlineData("USD", null, 840)]        // Null Name
    public void Validate_WithEmptyRequiredFields_ShouldHaveErrors(string code, string name, int codeiso)
    {
        // Arrange
        var command = new CreateCurrencyCommand(code, name, "دولار أمريكي", "US Dollar", codeiso);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (string.IsNullOrEmpty(code))
            result.ShouldHaveValidationErrorFor(c => c.Code);

        if (string.IsNullOrEmpty(name))
            result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData(-1)]      // Negative number
    [InlineData(1000)]    // 4-digit number
    [InlineData(9999)]    // More than 3 digits
    public void Validate_WithInvalidCodeIso_ShouldHaveErrors(int codeiso)
    {
        // Arrange
        var command = new CreateCurrencyCommand("USD", "US Dollar", "دولار أمريكي", "US Dollar", codeiso);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CodeIso);
    }
}