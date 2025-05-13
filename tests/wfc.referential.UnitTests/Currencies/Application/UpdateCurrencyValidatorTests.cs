using FluentValidation.TestHelper;
using wfc.referential.Application.Currencies.Commands.UpdateCurrency;
using Xunit;

namespace wfc.referential.UnitTests.Currencies.Application;

public class UpdateCurrencyValidatorTests
{
    private readonly UpdateCurrencyValidator _validator;

    public UpdateCurrencyValidatorTests()
    {
        _validator = new UpdateCurrencyValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateCurrencyCommand(
            Guid.NewGuid(),
            "USD",
            "US Dollar",
            true,
            "دولار أمريكي",
            "US Dollar",
            840);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "US Dollar", null, null, 840)]       // Empty Code
    [InlineData(null, "US Dollar", null, null, 840)]     // Null Code
    [InlineData("USD", "", null, null, 840)]             // Empty Name
    [InlineData("USD", null, null, null, 840)]           // Null Name
    public void Validate_WithEmptyRequiredFields_ShouldHaveErrors(string code, string name, string codeAR, string codeEN, int codeiso)
    {
        // Arrange
        var command = new UpdateCurrencyCommand(
            Guid.NewGuid(),
            code,
            name,
            true,
            codeAR,
            codeEN,
            codeiso);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (string.IsNullOrEmpty(code))
            result.ShouldHaveValidationErrorFor(c => c.Code);

        if (string.IsNullOrEmpty(name))
            result.ShouldHaveValidationErrorFor(c => c.Name);

        if (string.IsNullOrEmpty(codeAR))
            result.ShouldHaveValidationErrorFor(c => c.CodeAR);

        if (string.IsNullOrEmpty(codeEN))
            result.ShouldHaveValidationErrorFor(c => c.CodeEN);
    }

    [Theory]
    [InlineData(-1)]      // Negative number
    [InlineData(1000)]    // 4-digit number
    [InlineData(9999)]    // More than 3 digits
    public void Validate_WithInvalidCodeIso_ShouldHaveErrors(int codeiso)
    {
        // Arrange
        var command = new UpdateCurrencyCommand(
            Guid.NewGuid(),
            "USD",
            "US Dollar",
            true,
            "دولار أمريكي",
            "US Dollar",
            codeiso);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CodeIso);
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveErrors()
    {
        // Arrange
        var command = new UpdateCurrencyCommand(
            Guid.Empty,
            "USD",
            "US Dollar",
            true,
            "دولار أمريكي",
            "US Dollar",
            840);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CurrencyId);
    }
}