using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Hl7.Fhir.Model;
using Moq;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Helpers;
using WCCG.PAS.Referrals.API.Mappers;
using WCCG.PAS.Referrals.API.Repositories;
using WCCG.PAS.Referrals.API.Services;
using WCCG.PAS.Referrals.API.Unit.Tests.Extensions;
using Task = System.Threading.Tasks.Task;

namespace WCCG.PAS.Referrals.API.Unit.Tests.Services;

public class ReferralServiceTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();

    private readonly ReferralService _sut;

    public ReferralServiceTests()
    {
        _sut = _fixture.CreateWithFrozen<ReferralService>();
    }

    [Fact]
    public async Task CreateReferralAsyncShouldMapFromBundle()
    {
        //Arrange
        var bundle = _fixture.Create<Bundle>();

        //Act
        await _sut.CreateReferralAsync(bundle);

        //Assert
        _fixture.Mock<IReferralMapper>().Verify(x => x.MapFromBundle(bundle));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldValidateModel()
    {
        //Arrange
        var bundle = _fixture.Create<Bundle>();
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(bundle);

        //Assert
        _fixture.Mock<IValidator<ReferralDbModel>>().Verify(x => x.ValidateAsync(referralDbModel, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldThrowValidationExceptionWhenValidationFailed()
    {
        //Arrange
        var bundle = _fixture.Create<Bundle>();
        var validationResult = _fixture.Build<ValidationResult>()
            .With(x => x.Errors, _fixture.CreateMany<ValidationFailure>().ToList)
            .Create();

        _fixture.Mock<IValidator<ReferralDbModel>>().Setup(x => x.ValidateAsync(It.IsAny<ReferralDbModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        //Act
        var action = async () => await _sut.CreateReferralAsync(bundle);

        //Assert
        (await action.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().BeEquivalentTo(validationResult.Errors);
    }

    [Fact]
    public async Task CreateReferralAsyncShouldAdjustBundleWithDataModel()
    {
        //Arrange
        var bundle = _fixture.Create<Bundle>();
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(bundle);

        //Assert
        _fixture.Mock<IBundleFiller>().Verify(x => x.AdjustBundleWithDbModelData(bundle, referralDbModel));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldCreateReferralInDb()
    {
        //Arrange
        var bundle = _fixture.Create<Bundle>();
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(bundle);

        //Assert
        _fixture.Mock<IReferralCosmosRepository>().Verify(x => x.CreateReferralAsync(referralDbModel));
    }
}
