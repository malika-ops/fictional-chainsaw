using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using AutoFixture;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Constants;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxTests.DeleteTests;

public class DeleteTaxEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private const string BaseUrl = "api/taxes";

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns true when tax is deleted successfully")]
    public async Task Delete_ShouldReturnTrue_WhenTaxExists()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        var tax = Tax.Create(
            TaxId.Of(taxId),
            "01",
            "testAAB",
            "aa",
            "description",
            43,
            20);

        _taxRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Tax, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tax);

        _taxRuleDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaxRuleDetail>());

        _taxRepoMock.Setup(r => r.Update(It.IsAny<Tax>()));
        _taxRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _taxRepoMock.Verify(r => r.Update(It.Is<Tax>(t => t.IsEnabled == false)), Times.Once);
        _taxRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.Tax.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns 404 when tax does not exist")]
    public async Task Delete_ShouldReturn404_WhenTaxDoesNotExist()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        _taxRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Tax, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tax)null); // Tax not found

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _taxRepoMock.Verify(r => r.Update(It.IsAny<Tax>()), Times.Never);
    }

    [Fact(DisplayName = $"DELETE {BaseUrl}/id returns 400 when tax has tax rule details")]
    public async Task Delete_ShouldReturn400_WhenTaxHasTaxRuleDetails()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        var tax = Tax.Create(
            TaxId.Of(taxId),
            "01",
            "testAAB",
            "aa",
            "description",
            43,
            20);

        _taxRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Tax, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tax);

        var taxRuleDetail = _fixture.Create<TaxRuleDetail>();

        _taxRuleDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaxRuleDetail> { taxRuleDetail });

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{taxId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify that Update was never called
        _taxRepoMock.Verify(r => r.Update(It.IsAny<Tax>()), Times.Never);
    }
}