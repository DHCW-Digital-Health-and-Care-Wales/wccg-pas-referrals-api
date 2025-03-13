using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using WCCG.PAS.Referrals.API.Mappers;
using WCCG.PAS.Referrals.API.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.API.Unit.Tests.Mappers;

public class ReferralMapperTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();

    private readonly ReferralMapper _sut;

    private readonly Bundle _bundle;

    public ReferralMapperTests()
    {
        _sut = _fixture.CreateWithFrozen<ReferralMapper>();
        var bundleJson = File.ReadAllText("TestData/example-bundle.json");

        var options = new JsonSerializerOptions()
            .ForFhir(ModelInfo.ModelInspector)
            .UsingMode(DeserializerModes.BackwardsCompatible);
        _bundle = JsonSerializer.Deserialize<Bundle>(bundleJson, options)!;
    }

    [Fact]
    public void MapFromBundleShouldMapNhsNumber()
    {
        //Arrange
        const string expectedValue = "9449305552";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.NhsNumber.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapReferrer()
    {
        //Arrange
        const string expectedValue = "G1234567";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.Referrer.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapReferrerAddress()
    {
        //Arrange
        const string expectedValue = "W12345";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.ReferrerAddress.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapPatientGpCode()
    {
        //Arrange
        const string expectedValue = "G1234567";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.PatientGpCode.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapPatientGpPracticeCode()
    {
        //Arrange
        const string expectedValue = "W12345";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.PatientGpPracticeCode.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapPostcode()
    {
        //Arrange
        const string expectedValue = "CF14 4XW";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.PatientPostcode.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapPatientHealthBoardAreaCode()
    {
        //Arrange
        const string expectedValue = "123";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.PatientHealthBoardAreaCode.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapReferrerSourceType()
    {
        //Arrange
        const string expectedValue = "DE";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.ReferrerSourceType.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapLetterPriority()
    {
        //Arrange
        const string expectedValue = "R";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.LetterPriority.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapReferralAssignedConsultant()
    {
        //Arrange
        const string expectedValue = "SMITH";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.ReferralAssignedConsultant.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapReferralAssignedLocation()
    {
        //Arrange
        const string expectedValue = "HOSP1";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.ReferralAssignedLocation.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapPatientCategory()
    {
        //Arrange
        const string expectedValue = "01";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.PatientCategory.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapSpecialityIdentifier()
    {
        //Arrange
        const string expectedValue = "CARDIO";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.SpecialityIdentifier.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapRepeatPeriod()
    {
        //Arrange
        const string expectedValue = "6D";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.RepeatPeriod.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapFirstAppointmentDate()
    {
        //Arrange
        var expectedValue = PrimitiveTypeConverter.ConvertTo<DateTimeOffset>("2025-03-05T09:14:12.8371094Z");

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.FirstAppointmentDate.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapHealthRiskFactor()
    {
        //Arrange
        const string expectedValue = "0";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.HealthRiskFactor.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapCreationDate()
    {
        //Arrange
        var expectedValue = PrimitiveTypeConverter.ConvertTo<DateTimeOffset>("2025-02-19T09:14:12.8360738Z");

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.CreationDate.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldMapPriority()
    {
        //Arrange
        const string expectedValue = "R";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.Priority.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldSetWaitingList()
    {
        //Arrange
        const string expectedValue = "O";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.WaitingList.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldSetIntendedManagement()
    {
        //Arrange
        const string expectedValue = "6";

        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.IntendedManagement.Should().Be(expectedValue);
    }

    [Fact]
    public void MapFromBundleShouldCreateGuidForCaseNumber()
    {
        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.CaseNumber.Should().NotBeEmpty();
    }

    [Fact]
    public void MapFromBundleShouldCreateGuidForReferralId()
    {
        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.ReferralId.Should().NotBeEmpty();
    }

    [Fact]
    public void MapFromBundleShouldSetCurrentDateForHealthBoardReceiveDate()
    {
        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.HealthBoardReceiveDate.Should().NotBeNull();
    }

    [Fact]
    public void MapFromBundleShouldSetCurrentDateForBookingDate()
    {
        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.BookingDate.Should().NotBeNull();
    }

    [Fact]
    public void MapFromBundleShouldSetCurrentDateForTreatmentDate()
    {
        //Act
        var result = _sut.MapFromBundle(_bundle);

        //Assert
        result.Should().NotBeNull();
        result.TreatmentDate.Should().NotBeNull();
    }
}
