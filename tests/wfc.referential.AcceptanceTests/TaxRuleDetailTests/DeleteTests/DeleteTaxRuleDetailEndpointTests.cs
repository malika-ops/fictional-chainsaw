using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests;

public class DeleteTaxRuleDetailEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private const string BaseUrl = "api/tax-rule-details";

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns true when TaxRuleDetail is deleted successfully")]
    public async Task Delete_ShouldReturnTrue_WhenTaxRuleDetailExists()
    {
        // Arrange
        var taxRuleDetailId = Guid.NewGuid();

        var taxRuleDetail = TaxRuleDetail.Create(
            TaxRuleDetailsId.Of(taxRuleDetailId),
            corridorId: CorridorId.Of(Guid.NewGuid()),
            taxId: TaxId.Of(Guid.NewGuid()),
            serviceId: ServiceId.Of(Guid.NewGuid()),
            appliedOn: ApplicationRule.Amount);

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxRuleDetail);

        _taxRuleDetailsRepoMock.Setup(r => r.Update(It.IsAny<TaxRuleDetail>()));

        _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxRuleDetailId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _taxRuleDetailsRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()), Times.Once);
        _taxRuleDetailsRepoMock.Verify(r => r.Update(It.Is<TaxRuleDetail>(trd => trd.Id.Value == taxRuleDetailId && trd.IsEnabled == false)), Times.Once);
    }

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns 404 when TaxRuleDetail does not exist")]
    public async Task Delete_ShouldReturn404_WhenTaxRuleDetailDoesNotExist()
    {
        // Arrange
        var taxRuleDetailId = Guid.NewGuid();

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRuleDetail)null);

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxRuleDetailId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _taxRuleDetailsRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()), Times.Once);
        _taxRuleDetailsRepoMock.Verify(r => r.Update(It.IsAny<TaxRuleDetail>()), Times.Never);
    }
}
