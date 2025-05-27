using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.PatchTests;

public class PatchSupportAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repoMock = new();

    public PatchSupportAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISupportAccountRepository>();

                _repoMock.Setup(r => r.Update(It.IsAny<SupportAccount>()));
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
            });
        });
        _client = customisedFactory.CreateClient();
    }

    private static SupportAccount CreateTestSupportAccount(Guid id, string code, string description, decimal threshold, decimal limit, decimal balance)
    {
        return SupportAccount.Create(
            SupportAccountId.Of(id),
            code,
            description,
            threshold,
            limit,
            balance,
            "ACC" + code
        );
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Old Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount)null);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002",
            Threshold = 12000.00m,
            Limit = 20000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.IsAny<SupportAccount>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} allows updating only the enabled status")]
    public async Task Patch_ShouldAllowChangingOnlyEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Test Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        var payload = new
        {
            SupportAccountId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.IsAny<SupportAccount>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} returns 404 when account doesn't exist")]
    public async Task Patch_ShouldReturn404_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount)null);

        var payload = new
        {
            SupportAccountId = id,
            Threshold = 15000.00m,
            Limit = 25000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _repoMock.Verify(r => r.Update(It.IsAny<SupportAccount>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} returns 409 when new code already exists")]
    public async Task Patch_ShouldReturn409_WhenNewCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var existing = CreateTestSupportAccount(existingId, "SA002", "Existing Support Account", 12000.00m, 18000.00m, 6000.00m);
        var target = CreateTestSupportAccount(id, "SA001", "Target Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002"  // This code already exists for another account
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _repoMock.Verify(r => r.Update(It.IsAny<SupportAccount>()), Times.Never);
    }
}