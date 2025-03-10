using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Hl7.Fhir.Model;
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
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions().ForFhirExtended();
    private readonly string _bundleJson = File.ReadAllText("TestData/example-bundle.json");

    [Fact]
    public async Task CreateReferralAsyncShouldMapFromBundle()
    {
        //Arrange
        var sut = CreateReferralService();

        //Act
        await sut.CreateReferralAsync(_bundleJson);

        //Assert
        _fixture.Mock<IReferralMapper>().Verify(x => x.MapFromBundle(It.IsAny<Bundle>()));
    }

    [Fact]
    public async Task CreateReferralAsyncShouldValidateModel()
    {
        //Arrange
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        var sut = CreateReferralService();

        //Act
        await sut.CreateReferralAsync(_bundleJson);

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

        var sut = CreateReferralService();

        //Act
        var action = async () => await sut.CreateReferralAsync(_bundleJson);

        //Assert
        (await action.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().BeEquivalentTo(validationResult.Errors);
    }

    [Fact]
    public async Task CreateReferralAsyncShouldAdjustBundleWithDataModel()
    {
        //Arrange
        var referralDbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.BookingDate, DateTimeOffset.UtcNow.ToString("O"))
            .Create();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        var sut = CreateReferralService();

        //Act
        var result = await sut.CreateReferralAsync(_bundleJson);

        //Assert
        var bundle = JsonSerializer.Deserialize<Bundle>(result, _jsonSerializerOptions)!;

        var newReferralId = GetReferralIdFromBundle(bundle);
        newReferralId.Should().Be(referralDbModel.ReferralId);

        var newCaseNumber = GetCaseNumberFromBundle(bundle);
        newCaseNumber.Should().Be(referralDbModel.CaseNumber);

        var newBookingDate = GetBookingDateFromBundle(bundle);
        newBookingDate.Should().Be(referralDbModel.BookingDate);
    }

    [Fact]
    public async Task CreateReferralAsyncShouldCreateReferralInDb()
    {
        //Arrange
        var referralDbModel = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<IReferralMapper>().Setup(x => x.MapFromBundle(It.IsAny<Bundle>()))
            .Returns(referralDbModel);

        var sut = CreateReferralService();

        //Act
        await sut.CreateReferralAsync(_bundleJson);

        //Assert
        _fixture.Mock<IReferralCosmosRepository>().Verify(x => x.CreateReferralAsync(referralDbModel));
    }

    private static string GetReferralIdFromBundle(Bundle bundle)
    {
        var serviceRequest = bundle.GetResourceByType<ServiceRequest>()!;
        return serviceRequest.Identifier.SelectWithCondition(x => x.System, FhirConstants.ReferralIdSystem)!.Value;
    }

    private static string GetCaseNumberFromBundle(Bundle bundle)
    {
        var serviceRequest = bundle.GetResourceByType<ServiceRequest>()!;
        var patient = bundle.GetResourceByUrl<Patient>(serviceRequest.Subject.Reference)!;
        return patient.Identifier
            .SelectWithCondition(x => x.System, FhirConstants.PasIdentifierSystem)
            !.Value;
    }

    private static string GetBookingDateFromBundle(Bundle bundle)
    {
        var serviceRequest = bundle.GetResourceByType<ServiceRequest>()!;
        var encounter = bundle.GetResourceByUrl<Encounter>(serviceRequest.Encounter.Reference)!;
        var appointment = bundle.GetResourceByUrl<Appointment>(encounter.Appointment.FirstOrDefault()!.Reference)!;
        return appointment.Created;
    }

    private ReferralService CreateReferralService()
    {
        return new ReferralService(
            _fixture.Mock<IReferralMapper>().Object,
            _fixture.Mock<IReferralCosmosRepository>().Object,
            _jsonSerializerOptions,
            _fixture.Mock<IValidator<ReferralDbModel>>().Object);
    }
}
