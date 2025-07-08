using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Taxes.Dtos;
using wfc.referential.Domain.TaxAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxTests.PatchTests;

public class PatchTaxEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private const string BaseUrl = "api/taxes";

    [Fact(DisplayName = $"PATCH {BaseUrl}/id updates the Tax successfully")]
    public async Task PatchTax_ShouldReturnUpdatedTaxId_WhenTaxExists()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        var patchRequest = new PatchTaxRequest
        {
            Code = "testAAB",
            CodeEn = "TestAABEN",
            CodeAr = "testAABAR",
        };

        var tax = Tax.Create(
            TaxId.Of(taxId),
            "TX001",
            "VAT",
            "ضريبة القيمة المضافة",
            "Value Added Tax applicable to goods and services",
            43,
            15
        );

        _taxRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Tax, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tax);

        // Act
        var response = await _client.PatchAsync($"{BaseUrl}/{taxId}", JsonContent.Create(patchRequest));
        var updatedTaxId = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedTaxId.Should().Be(true);
        tax.Code.Should().BeEquivalentTo(patchRequest.Code);
        tax.CodeEn.Should().BeEquivalentTo(patchRequest.CodeEn);
        tax.CodeAr.Should().BeEquivalentTo(patchRequest.CodeAr);
    }


    [Fact(DisplayName = $"PATCH {BaseUrl}/id returns 404 when tax does not exist")]
    public async Task PatchTax_ShouldReturnNotFound_WhenTaxDoesNotExist()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        var patchRequest = new PatchTaxRequest
        {
            Code = "testAAB",
            CodeEn = "TestAABEN",
        };

        _taxRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Tax, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tax)null); 

        // Act
        var response = await _client.PatchAsync($"{BaseUrl}/{taxId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = $"PATCH {BaseUrl}/id returns 400 when validation fails")]
    public async Task PatchTax_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var taxId = Guid.NewGuid();
        var patchRequest = new PatchTaxRequest
        {
            Code = "", // Assuming empty code is invalid
            CodeEn = "TestAABEN",
        };
        // Act
        var response = await _client.PatchAsync($"{BaseUrl}/{taxId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
