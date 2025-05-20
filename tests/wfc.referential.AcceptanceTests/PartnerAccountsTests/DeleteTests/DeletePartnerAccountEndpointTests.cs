using BuildingBlocks.Core.Exceptions;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;
using Xunit;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.UnitTests.Application.Commands.PartnerAccounts.DeletePartnerAccount;

public class DeletePartnerAccountCommandHandlerTests
{
    private readonly Mock<IPartnerAccountRepository> _partnerAccountRepoMock = new();
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();
    private readonly DeletePartnerAccountCommandHandler _handler;

    public DeletePartnerAccountCommandHandlerTests()
    {
        _handler = new DeletePartnerAccountCommandHandler(_partnerAccountRepoMock.Object, _partnerRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDisableAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(accountId);

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(partnerAccount);

        _partnerRepoMock.Setup(r => r.GetAllPartnersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Partner>());

        // Act
        var result = await _handler.Handle(new DeletePartnerAccountCommand(accountId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        partnerAccount.IsEnabled.Should().BeFalse();

        _partnerAccountRepoMock.Verify(r => r.UpdatePartnerAccountAsync(
            It.Is<PartnerAccount>(p => p.Id.Value == accountId && p.IsEnabled == false),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.IsAny<PartnerAccountId>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((PartnerAccount)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidPartnerAccountDeletingException>(() =>
            _handler.Handle(new DeletePartnerAccountCommand(accountId), CancellationToken.None));

        _partnerAccountRepoMock.Verify(r => r.UpdatePartnerAccountAsync(
            It.IsAny<PartnerAccount>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenAccountIsLinkedToPartner()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(accountId);
        var partner = CreateTestPartner(Guid.NewGuid(), "TEST01", commissionAccountId: accountId);

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(partnerAccount);

        _partnerRepoMock.Setup(r => r.GetAllPartnersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Partner> { partner });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() =>
            _handler.Handle(new DeletePartnerAccountCommand(accountId), CancellationToken.None));

        _partnerAccountRepoMock.Verify(r => r.UpdatePartnerAccountAsync(
            It.IsAny<PartnerAccount>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private PartnerAccount CreateTestPartnerAccount(Guid id)
    {
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var accountType = ParamType.Create(ParamTypeId.Of(accountTypeId), null, "Activity");

        return PartnerAccount.Create(
            new PartnerAccountId(id),
            "000123456789",
            "12345678901234567890123",
            "Casablanca Centre",
            "Test Business",
            "TB",
            50000.00m,
            bank,
            accountType
        );
    }

    private Partner CreateTestPartner(
        Guid id,
        string code,
        Guid? commissionAccountId = null,
        Guid? activityAccountId = null)
    {
        return Partner.Create(
            new PartnerId(id),
            code,
            "Test Partner",
            NetworkMode.VRP,
            PaymentMode.PostPaye,
            "Partner Type",
            SupportAccountType.Individuel,
            "123456789",
            "IR",
            "AUX001",
            "ICE123456",
            "20%",
            "",
            null,
            commissionAccountId,
            activityAccountId,
            null
        );
    }
}