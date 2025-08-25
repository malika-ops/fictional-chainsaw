using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.CreateTests;

public class CreateSupportAccountEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "POST /api/support-accounts returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Code = "SA001",
            Description = "Support Account 1",
            Threshold = 10000.00m,
            Limit = 20000.00m,
            AccountBalance = 5000.00m,
            AccountingNumber = "ACC001",
            PartnerId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        _supportAccountRepoMock.Verify(r =>
            r.AddAsync(It.Is<SupportAccount>(s =>
                    s.Code == payload.Code &&
                    s.Description == payload.Description &&
                    s.Threshold == payload.Threshold &&
                    s.Limit == payload.Limit &&
                    s.AccountBalance == payload.AccountBalance &&
                    s.AccountingNumber == payload.AccountingNumber),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/support-accounts returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var invalidPayload = new
        {
            // Code intentionally omitted
            Description = "Support Account 1",
            Threshold = 10000.00m,
            Limit = 20000.00m,
            AccountBalance = 5000.00m,
            AccountingNumber = "ACC001",
            PartnerId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _supportAccountRepoMock.Verify(r => r.AddAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/support-accounts returns 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        const string duplicateCode = "SA001";
        var existingAccount = SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            duplicateCode,
            "Existing Support Account",
            10000.00m,
            15000.00m,
            5000.00m,
            "ACC001",
            SupportAccountTypeEnum.Individuel
        );

        _supportAccountRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var payload = new
        {
            Code = duplicateCode,
            Description = "New Support Account",
            Threshold = 15000.00m,
            Limit = 25000.00m,
            AccountBalance = 7500.00m,
            AccountingNumber = "ACC002",
            PartnerId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _supportAccountRepoMock.Verify(r => r.AddAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "POST /api/support-accounts validates all required fields")]
    [InlineData("", "Description", "AccountingNumber")]
    [InlineData("Code", "", "AccountingNumber")]
    [InlineData("Code", "Description", "")]
    public async Task Post_ShouldReturn400_WhenRequiredFieldsAreMissing(string code, string description, string accountingNumber)
    {
        // Arrange
        var invalidPayload = new
        {
            Code = code,
            Description = description,
            Threshold = 10000.00m,
            Limit = 20000.00m,
            AccountBalance = 5000.00m,
            AccountingNumber = accountingNumber,
            PartnerId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _supportAccountRepoMock.Verify(r => r.AddAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}