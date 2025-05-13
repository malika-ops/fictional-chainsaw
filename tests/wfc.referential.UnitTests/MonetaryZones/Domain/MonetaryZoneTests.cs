using wfc.referential.Domain.MonetaryZone.Events;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.UnitTests.MonetaryZones.Domain;

public class MonetaryZoneTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateMonetaryZone()
    {
        // Arrange
        var id = MonetaryZoneId.Of(Guid.NewGuid());
        string code = "EUR";
        string name = "Euro Zone";
        string description = "European Monetary Zone";

        // Act
        var monetaryZone = MonetaryZone.Create(id, code, name, description, null);

        // Assert
        Assert.Equal(id, monetaryZone.Id);
        Assert.Equal(code, monetaryZone.Code);
        Assert.Equal(name, monetaryZone.Name);
        Assert.Equal(description, monetaryZone.Description);

        // Check that the creation event was raised
        var domainEvent = Assert.Single(monetaryZone.DomainEvents);
        var createdEvent = Assert.IsType<MonetaryZoneCreatedEvent>(domainEvent);
        Assert.Equal(id.Value, createdEvent.MonetaryZoneId);
        Assert.Equal(code, createdEvent.Code);
        Assert.Equal(name, createdEvent.Name);
        Assert.Equal(description, createdEvent.Description);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateMonetaryZone()
    {
        // Arrange
        var id = MonetaryZoneId.Of(Guid.NewGuid());
        var monetaryZone = MonetaryZone.Create(id, "OLD", "Old Name", "Old Description", null);
        monetaryZone.ClearDomainEvents(); //Ignore the creation event

        string newCode = "NEW";
        string newName = "New Name";
        string newDescription = "New Description";

        // Act
        monetaryZone.Update(newCode, newName, newDescription, true);

        // Assert
        Assert.Equal(newCode, monetaryZone.Code);
        Assert.Equal(newName, monetaryZone.Name);
        Assert.Equal(newDescription, monetaryZone.Description);

        // Check that the update event was raised
        var domainEvent = Assert.Single(monetaryZone.DomainEvents);
        var updatedEvent = Assert.IsType<MonetaryZoneUpdatedEvent>(domainEvent);
        Assert.Equal(id.Value, updatedEvent.MonetaryZoneId);
        Assert.Equal(newCode, updatedEvent.Code);
        Assert.Equal(newName, updatedEvent.Name);
        Assert.Equal(newDescription, updatedEvent.Description);
    }

    [Fact]
    public void Patch_ShouldRaisePatchedEvent()
    {
        // Arrange
        var id = MonetaryZoneId.Of(Guid.NewGuid());
        var monetaryZone = MonetaryZone.Create(id, "CODE", "Name", "Description", null);
        monetaryZone.ClearDomainEvents(); // Ignore the creation event

        // Act
        monetaryZone.Patch();

        // Assert
        var domainEvent = Assert.Single(monetaryZone.DomainEvents);
        var patchedEvent = Assert.IsType<MonetaryZonePatchedEvent>(domainEvent);
        Assert.Equal(id.Value, patchedEvent.MonetaryZoneId);
        Assert.Equal(monetaryZone.Code, patchedEvent.Code);
        Assert.Equal(monetaryZone.Name, patchedEvent.Name);
        Assert.Equal(monetaryZone.Description, patchedEvent.Description);
    }
}