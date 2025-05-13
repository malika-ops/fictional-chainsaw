using FluentValidation.TestHelper;
using Moq;
using wfc.referential.Application.Currencies.Commands.PatchCurrency;
using wfc.referential.Application.Interfaces;
using Xunit;

namespace wfc.referential.UnitTests.Currencies.Application;

public class PatchCurrencyValidatorTests
{
    private readonly PatchCurrencyValidator _validator;
    private readonly Mock<ICurrencyRepository> _repositoryMock;

    public PatchCurrencyValidatorTests()
    {
        _repositoryMock = new Mock<ICurrencyRepository>();
        _validator = new PatchCurrencyValidator(_repositoryMock.Object);
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new PatchCurrencyCommand(
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

    [Fact]
    public void Validate_WithOnlyCode_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new PatchCurrencyCommand(
            Guid.NewGuid(),
            "USD",
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOnlyName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new PatchCurrencyCommand(
            Guid.NewGuid(),
            null,
            "US Dollar",
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOnlyIsEnabled_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new PatchCurrencyCommand(
            Guid.NewGuid(),
            null,
            null,
            false);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOnlyCodeIso_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new PatchCurrencyCommand(
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null,
            840);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCode_ShouldHaveErrors()
    {
        // Arrange
        var command = new PatchCurrencyCommand(
            Guid.NewGuid(),
            "",
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Code);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveErrors()
    {
        // Arrange
        var command = new PatchCurrencyCommand(
            Guid.NewGuid(),
            null,
            "",
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData(-1)]      // Negative number
    [InlineData(1000)]    // 4-digit number
    [InlineData(9999)]    // More than 3 digits
    public void Validate_WithInvalidCodeIso_ShouldHaveErrors(int codeiso)
    {
        // Arrange
        var command = new PatchCurrencyCommand(
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null,
            codeiso);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("CodeIso.Value");
    }
}