using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Pages;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Pages;

public class ItemEditorModelTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();
    private readonly ItemEditorModel _sut;

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private readonly Referral _referral;

    public ItemEditorModelTests()
    {
        _referral = _fixture.Create<Referral>();

        _sut = new ItemEditorModel(_fixture.Mock<IReferralService>().Object, _fixture.Mock<IValidator<Referral>>().Object)
        {
            ReferralJson = JsonSerializer.Serialize(_referral, _jsonOptions), ReferralId = _referral.Id!
        };
    }

    [Fact]
    public async Task OnGetShouldCallGetByIdAsync()
    {
        //Arrange
        var expectedJson = JsonSerializer.Serialize(_referral, _jsonOptions);

        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(_referral);
        //Act
        await _sut.OnGet(_referral.Id!);

        //Assert
        _sut.ReferralId.Should().Be(_referral.Id);
        _sut.ReferralJson.Should().Be(expectedJson);

        _fixture.Mock<IReferralService>().Verify(r => r.GetByIdAsync(_referral.Id!));
    }

    [Fact]
    public async Task OnPostShouldCallUpsertAsyncWhenDeserializedAndValidatedSuccessfully()
    {
        //Act
        await _sut.OnPost();

        //Assert
        _sut.IsSaved.Should().BeTrue();

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.Is<Referral>(r => r.IsEquivalentTo(_referral))));
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenDeserializationFailed()
    {
        //Arrange
        var invalidReferral = _fixture.Create<string>();
        _sut.ReferralJson = JsonSerializer.Serialize(invalidReferral, _jsonOptions);

        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(_referral);

        //Act
        await _sut.OnPost();

        //Assert
        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().NotBeEmpty();

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.IsAny<Referral>()), Times.Never());
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenValidationFailed()
    {
        //Arrange
        var validationResult = _fixture.Build<ValidationResult>()
            .With(x => x.Errors, _fixture.CreateMany<ValidationFailure>().ToList)
            .Create();
        var expectedErrorMessage = validationResult.Errors.Select(x => x.ErrorMessage).Aggregate((f, s) => f + "<br/>" + s);

        _fixture.Mock<IValidator<Referral>>().Setup(r => r.ValidateAsync(It.IsAny<Referral>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        //Act
        await _sut.OnPost();

        //Assert
        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().Be(expectedErrorMessage);

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.IsAny<Referral>()), Times.Never());
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenUpsertFailed()
    {
        //Arrange
        var errorMessage = _fixture.Create<string>();

        _fixture.Mock<IReferralService>().Setup(s => s.UpsertAsync(It.IsAny<Referral>()))
            .ThrowsAsync(new ArgumentException(errorMessage));

        //Act
        await _sut.OnPost();

        //Assert
        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().Be(errorMessage);

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.Is<Referral>(r => r.IsEquivalentTo(_referral))));
    }
}
