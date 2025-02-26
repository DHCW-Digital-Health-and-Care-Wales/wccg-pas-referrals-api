using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Moq;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Extensions;
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

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        .ForFhir(ModelInfo.ModelInspector)
        .UsingMode(DeserializerModes.BackwardsCompatible);

    private readonly Bundle _bundle;

    public ReferralServiceTests()
    {
        _sut = _fixture.CreateWithFrozen<ReferralService>();

        var bundleJson = File.ReadAllText("TestData/example-bundle.json");
        _bundle = JsonSerializer.Deserialize<Bundle>(bundleJson, _options)!;
    }

    [Fact]
    public async Task CreateReferralAsyncShouldMapFromBundle()
    {
        //Act
        await _sut.CreateReferralAsync(_bundle);

        //Assert
        _fixture.Mock<IReferralMapper>().Verify(x => x.MapFromBundle(_bundle));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldValidateModel()
    {
        //Arrange
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(_bundle);

        //Assert
        _fixture.Mock<IValidator<ReferralDbModel>>().Verify(x => x.ValidateAsync(referralDbModel, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldThrowValidationExceptionWhenValidationFailed()
    {
        //Arrange
        var validationResult = _fixture.Build<ValidationResult>()
            .With(x => x.Errors, _fixture.CreateMany<ValidationFailure>().ToList)
            .Create();

        _fixture.Mock<IValidator<ReferralDbModel>>().Setup(x => x.ValidateAsync(It.IsAny<ReferralDbModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        //Act
        var action = async () => await _sut.CreateReferralAsync(_bundle);

        //Assert
        (await action.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().BeEquivalentTo(validationResult.Errors);
    }

    [Fact]
    public async Task CreateReferralAsyncShouldAdjustBundleWithDataModel()
    {
        //Arrange
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(_bundle);

        //Assert
        var newReferralId = GetReferralIdFromBundle();
        newReferralId.Should().Be(referralDbModel.ReferralId);

        var newCaseNumber = GetCaseNumberFromBundle();
        newCaseNumber.Should().Be(referralDbModel.CaseNumber);

        var newBookingDate = GetBookingDateFromBundle();
        newBookingDate.Should().Be(referralDbModel.BookingDate);
    }

    [Fact]
    public async Task CreateReferralAsyncShouldCreateReferralInDb()
    {
        //Arrange
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        //Act
        await _sut.CreateReferralAsync(_bundle);

        //Assert
        _fixture.Mock<IReferralCosmosRepository>().Verify(x => x.CreateReferralAsync(referralDbModel));
    }

    private string GetReferralIdFromBundle()
    {
        var serviceRequest = _bundle.GetResourceByType<ServiceRequest>()!;
        return serviceRequest.Identifier.SelectWithCondition(x => x.System, NhsFhirConstants.ReferralIdSystem)!.Value;
    }

    private string GetCaseNumberFromBundle()
    {
        var serviceRequest = _bundle.GetResourceByType<ServiceRequest>()!;
        var patient = _bundle.GetResourceByUrl<Patient>(serviceRequest.Subject.Reference)!;
        return patient.Identifier
            .SelectWithCondition(x => x.System, NhsFhirConstants.PasIdentifierSystem)
            !.Value;
    }

    private string GetBookingDateFromBundle()
    {
        var serviceRequest = _bundle.GetResourceByType<ServiceRequest>()!;
        var encounter = _bundle.GetResourceByUrl<Encounter>(serviceRequest.Encounter.Reference)!;
        var appointment = _bundle.GetResourceByUrl<Appointment>(encounter.Appointment.FirstOrDefault()!.Reference)!;
        return appointment.Created;
    }
}
