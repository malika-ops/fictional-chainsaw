using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.UnitTests.Regions;

public class RegionTests
{
    private readonly RegionId _regionId = new RegionId(Guid.NewGuid());
    private readonly CountryId _countryId = new CountryId(Guid.NewGuid());
    [Fact]
    public void Constructor_ShouldInitializeProperties_WhenValidValuesProvided()
    {
        // Arrange
        string expectedCode = "60";
        string expectedName = "Casablanca-Settat";

        // Act
        var region = Region.Create(_regionId, expectedCode, expectedName, _countryId);

        // Assert
        Assert.Equal(expectedCode, region.Code);
        Assert.Equal(expectedName, region.Name);
        Assert.Equal(_countryId, region.CountryId);
        Assert.Equal(true, region.IsEnabled);  // Default status
    }

    [Fact]
    public void Create_ShouldCreateRegionWithGivenParameters_WhenValidValuesProvided()
    {
        // Arrange
        string expectedCode = "60";
        string expectedName = "Casablanca-Settat";
        bool expectedStatus =   true; // Or any other status if required
            
        // Act
        var region = Region.Create(_regionId, expectedCode, expectedName, _countryId);

        // Assert
        Assert.Equal(expectedCode, region.Code);
        Assert.Equal(expectedName, region.Name);
        Assert.Equal(expectedStatus, region.IsEnabled);
        Assert.Equal(_countryId, region.CountryId);
    }

    [Fact]
    public void Constructor_ShouldSetDefaultStatus_WhenNoStatusProvided()
    {
        // Arrange
        string expectedCode = "US";
        string expectedName = "North America";

        // Act
        var region = Region.Create(_regionId, expectedCode, expectedName, _countryId);

        // Assert
        Assert.Equal(true, region.IsEnabled);  // Default status value
    }
}
