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
    public async Task CreateReferralAsyncShouldDeserializeBundle()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();

        //Act
        await _sut.CreateReferralAsync(bundleJson);

        //Assert
        _fixture.Mock<IFhirSerializer>().Verify(x => x.Deserialize<Bundle>(bundleJson));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldReturnNullWhenSerializerReturnsNull()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();

        _fixture.Mock<IFhirSerializer>().Setup(x => x.Deserialize<Bundle>(It.IsAny<string>()))
            .Returns((Bundle?)null);

        //Act
        var result = await _sut.CreateReferralAsync(bundleJson);

        //Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateReferralAsyncShouldMapFromBundle()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();
        var bundle = _fixture.Create<Bundle>();

        _fixture.Mock<IFhirSerializer>().Setup(x => x.Deserialize<Bundle>(It.IsAny<string>()))
            .Returns(bundle);
        //Act
        await _sut.CreateReferralAsync(bundleJson);

        //Assert
        _fixture.Mock<IReferralMapper>().Verify(x => x.MapFromBundle(bundle));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldReturnNullWhenMapFromBundleReturnsNull()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns((ReferralDbModel?)null);

        //Act
        var result = await _sut.CreateReferralAsync(bundleJson);

        //Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateReferralAsyncShouldValidateModel()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(bundleJson);

        //Assert
        _fixture.Mock<IValidator<ReferralDbModel>>().Verify(x => x.ValidateAsync(referralDbModel, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldThrowValidationExceptionWhenValidationFailed()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();
        var validationResult = _fixture.Build<ValidationResult>()
            .With(x => x.Errors, _fixture.CreateMany<ValidationFailure>().ToList)
            .Create();

        _fixture.Mock<IValidator<ReferralDbModel>>().Setup(x => x.ValidateAsync(It.IsAny<ReferralDbModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        //Act
        var action = async () => await _sut.CreateReferralAsync(bundleJson);

        //Assert
        (await action.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().BeEquivalentTo(validationResult.Errors);
    }

    [Fact]
    public async Task CreateReferralAsyncShouldAdjustBundleWithDataModel()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();
        var referralDbModel = _fixture.Create<ReferralDbModel>();
        var bundle = _fixture.Create<Bundle>();

        _fixture.Mock<IFhirSerializer>().Setup(x => x.Deserialize<Bundle>(It.IsAny<string>()))
            .Returns(bundle);
        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(bundleJson);

        //Assert
        _fixture.Mock<IBundleFiller>().Verify(x => x.AdjustBundleWithDbModelData(bundle, referralDbModel));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldCreateReferralInDb()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(bundleJson);

        //Assert
        _fixture.Mock<IReferralCosmosRepository>().Verify(x => x.CreateReferralAsync(referralDbModel));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldReturnSerializedBundle()
    {
        //Arrange
        var bundleJson = _fixture.Create<string>();
        var bundle = _fixture.Create<Bundle>();
        var outputBundleJson = _fixture.Create<string>();

        _fixture.Mock<IFhirSerializer>().Setup(x => x.Deserialize<Bundle>(It.IsAny<string>()))
            .Returns(bundle);

        _fixture.Mock<IFhirSerializer>().Setup(x => x.Serialize(bundle))
            .Returns(outputBundleJson);

        //Act
        var result = await _sut.CreateReferralAsync(bundleJson);

        //Assert
        result.Should().Be(outputBundleJson);

        _fixture.Mock<IFhirSerializer>().Verify(x => x.Serialize(bundle));
    }
}
