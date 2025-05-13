using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Events;
using Xunit;

namespace wfc.referential.UnitTests.Currencies.Domain;

public class CurrencyTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateCurrency()
    {
        // Arrange
        var id = CurrencyId.Of(Guid.NewGuid());
        string code = "USD";
        string name = "US Dollar";
        string codeAR = "دولار أمريكي";
        string codeEN = "US Dollar";
        int codeiso = 840;

        // Act
        var currency = Currency.Create(id, code, codeAR, codeEN, name, codeiso);

        // Assert
        Assert.Equal(id, currency.Id);
        Assert.Equal(code, currency.Code);
        Assert.Equal(name, currency.Name);
        Assert.Equal(codeiso, currency.CodeIso);
        Assert.True(currency.IsEnabled);

        // Check that the creation event was raised
        var domainEvent = Assert.Single(currency.DomainEvents);
        var createdEvent = Assert.IsType<CurrencyCreatedEvent>(domainEvent);
        Assert.Equal(id.Value, createdEvent.CurrencyId);
        Assert.Equal(code, createdEvent.Code);
        Assert.Equal(name, createdEvent.Name);
        Assert.Equal(codeiso, createdEvent.CodeIso);
        Assert.True(createdEvent.IsEnabled);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateCurrency()
    {
        // Arrange
        var id = CurrencyId.Of(Guid.NewGuid());
        var currency = Currency.Create(id, "OLD", "دولار أمريكي", "Old Name", "Old Name", 123);
        currency.ClearDomainEvents(); // Ignore the creation event

        string newCode = "NEW";
        string newName = "New Name";
        string newCodeAR = "رمز جديد";
        string newCodeEN = "New Code";
        int newCodeIso = 456;

        // Act
        currency.Update(newCode, newCodeAR, newCodeEN, newName, newCodeIso);
        currency.Disable(); // Test disabling separately

        // Assert
        Assert.Equal(newCode, currency.Code);
        Assert.Equal(newName, currency.Name);
        Assert.Equal(newCodeIso, currency.CodeIso);
        Assert.False(currency.IsEnabled);
        Assert.Equal(newCodeAR, currency.CodeAR);
        Assert.Equal(newCodeEN, currency.CodeEN);

        // Check that the update event was raised
        Assert.Equal(2, currency.DomainEvents.Count);
        var updatedEvent = Assert.IsType<CurrencyUpdatedEvent>(currency.DomainEvents[0]);
        Assert.Equal(id.Value, updatedEvent.CurrencyId);
        Assert.Equal(newCode, updatedEvent.Code);
        Assert.Equal(newName, updatedEvent.Name);
        Assert.Equal(newCodeIso, updatedEvent.CodeIso);
        Assert.True(updatedEvent.IsEnabled); // IsEnabled won't be changed by Update
        Assert.Equal(newCodeAR, updatedEvent.CodeAR);
        Assert.Equal(newCodeEN, updatedEvent.CodeEN);

        var disabledEvent = Assert.IsType<CurrencyDisabledEvent>(currency.DomainEvents[1]);
        Assert.Equal(id.Value, disabledEvent.CurrencyId);
    }

    [Fact]
    public void Patch_ShouldRaisePatchedEvent()
    {
        // Arrange
        var id = CurrencyId.Of(Guid.NewGuid());
        var currency = Currency.Create(id, "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840);
        currency.ClearDomainEvents(); // Ignore the creation event

        // Act
        currency.Patch(currency.Code, currency.CodeAR, currency.CodeEN, currency.Name, currency.CodeIso);

        // Assert
        var domainEvent = Assert.Single(currency.DomainEvents);
        var patchedEvent = Assert.IsType<CurrencyPatchedEvent>(domainEvent);
        Assert.Equal(id.Value, patchedEvent.CurrencyId);
        Assert.Equal(currency.Code, patchedEvent.Code);
        Assert.Equal(currency.Name, patchedEvent.Name);
        Assert.Equal(currency.CodeIso, patchedEvent.CodeIso);
        Assert.Equal(currency.IsEnabled, patchedEvent.IsEnabled);
    }

    [Fact]
    public void Disable_ShouldDisableCurrency()
    {
        // Arrange
        var id = CurrencyId.Of(Guid.NewGuid());
        var currency = Currency.Create(id, "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840);
        currency.ClearDomainEvents(); // Ignore the creation event

        // Act
        currency.Disable();

        // Assert
        Assert.False(currency.IsEnabled);

        // Check that the status changed event was raised
        var domainEvent = Assert.Single(currency.DomainEvents);
        var disabledEvent = Assert.IsType<CurrencyDisabledEvent>(domainEvent);
        Assert.Equal(id.Value, disabledEvent.CurrencyId);
    }

    [Fact]
    public void Activate_ShouldActivateCurrency()
    {
        // Arrange
        var id = CurrencyId.Of(Guid.NewGuid());
        var currency = Currency.Create(id, "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840);
        currency.Disable(); // First disable
        currency.ClearDomainEvents(); // Ignore the previous events

        // Act
        currency.Activate();

        // Assert
        Assert.True(currency.IsEnabled);

        // Check that the status changed event was raised
        var domainEvent = Assert.Single(currency.DomainEvents);
        var activatedEvent = Assert.IsType<CurrencyActivatedEvent>(domainEvent);
        Assert.Equal(id.Value, activatedEvent.CurrencyId);
    }
}