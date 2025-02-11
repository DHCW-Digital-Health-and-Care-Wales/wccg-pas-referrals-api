using AutoFixture;
using FluentValidation.TestHelper;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;
using WCCG.PAS.Referrals.UI.Validators;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Validators;

public class ReferralValidatorTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();
    private readonly ReferralValidator _sut;

    public ReferralValidatorTests()
    {
        _sut = _fixture.CreateWithFrozen<ReferralValidator>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldContainErrorWhenIdInvalid(string id)
    {
        //Arrange
        var referral = _fixture.Build<Referral>()
            .With(x => x.Id, id)
            .Create();

        //Act
        var result = await _sut.TestValidateAsync(referral);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ShouldContainErrorWhenCaseNumberInvalid(string caseNumber)
    {
        //Arrange
        var referral = _fixture.Build<Referral>()
            .With(x => x.CaseNumber, caseNumber)
            .Create();

        //Act
        var result = await _sut.TestValidateAsync(referral);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.CaseNumber);
    }

    [Fact]
    public async Task ShouldNotContainErrorsWhenModelIsValid()
    {
        //Arrange
        var referral = _fixture.Create<Referral>();

        //Act
        var result = await _sut.TestValidateAsync(referral);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
