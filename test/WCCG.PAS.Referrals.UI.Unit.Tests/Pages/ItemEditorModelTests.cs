using AutoFixture;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Pages;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Pages;

public class ItemEditorModelTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();
    private readonly ItemEditorModel _sut;

    public ItemEditorModelTests()
    {
        _sut = new ItemEditorModel(_fixture.Mock<IReferralService>().Object);
    }

    [Fact]
    public async Task OnGet_Should_Call_GetByIdAsync()
    {
        //Arrange
        var id = _fixture.Create<string>();
        var referral = _fixture.Create<Referral>();

        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(referral);
        //Act
        await _sut.OnGet(id);

        //Assert
        _sut.Referral.Should().BeEquivalentTo(referral);

        _fixture.Mock<IReferralService>().Verify(r => r.GetByIdAsync(id));
    }

    [Fact]
    public async Task OnPost_Should_Call_UpsertAsync_When_DeserializedSuccessfully()
    {
        //Arrange
        var id = _fixture.Create<string>();
        var referral = _fixture.Create<Referral>();
        var referralJson = JsonConvert.SerializeObject(referral);

        //Act
        await _sut.OnPost(id, referralJson);

        //Assert
        _sut.Referral.Should().BeEquivalentTo(referral);
        _sut.IsSaved.Should().BeTrue();

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.Is<Referral>(r => r.IsEquivalentTo(referral))));
    }

    [Fact]
    public async Task OnPost_Should_Call_GetByIdAsync_When_DeserializationFailed()
    {
        //Arrange
        var id = _fixture.Create<string>();
        var invalidReferral = _fixture.Create<string>();
        var invalidReferralJson = JsonConvert.SerializeObject(invalidReferral);

        var originalReferral = _fixture.Create<Referral>();
        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(originalReferral);

        //Act
        await _sut.OnPost(id, invalidReferralJson);

        //Assert
        _sut.Referral.Should().BeEquivalentTo(originalReferral);
        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().NotBeEmpty();

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.IsAny<Referral>()), Times.Never());
        _fixture.Mock<IReferralService>().Verify(s => s.GetByIdAsync(id));
    }

    [Fact]
    public async Task OnPost_Should_Call_GetByIdAsync_When_UpsertFailed()
    {
        //Arrange
        var id = _fixture.Create<string>();
        var referral = _fixture.Create<Referral>();
        var referralJson = JsonConvert.SerializeObject(referral);

        var errorMessage = _fixture.Create<string>();

        var originalReferral = _fixture.Create<Referral>();
        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(originalReferral);

        _fixture.Mock<IReferralService>().Setup(s => s.UpsertAsync(It.IsAny<Referral>()))
            .ThrowsAsync(new Exception(errorMessage));

        //Act
        await _sut.OnPost(id, referralJson);

        //Assert
        _sut.Referral.Should().BeEquivalentTo(originalReferral);
        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().Be(errorMessage);

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.Is<Referral>(r => r.IsEquivalentTo(referral))));
        _fixture.Mock<IReferralService>().Verify(s => s.GetByIdAsync(id));
    }
}
