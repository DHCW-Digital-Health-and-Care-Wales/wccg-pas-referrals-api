using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Unit.Tests.Extensions;
using WCCG.PAS.Referrals.API.Validators;

namespace WCCG.PAS.Referrals.API.Unit.Tests.Validators;

public class ReferralDbModelValidatorTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();

    private readonly ReferralDbModelValidator _sut;

    public ReferralDbModelValidatorTests()
    {
        _sut = _fixture.CreateWithFrozen<ReferralDbModelValidator>();
        _sut.ClassLevelCascadeMode = CascadeMode.Continue;
        _sut.RuleLevelCascadeMode = CascadeMode.Stop;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    public void ShouldContainErrorWhenCaseNumberInvalid(string? caseNumber)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.CaseNumber, caseNumber)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.CaseNumber);
    }

    [Fact]
    public void ShouldNotContainErrorWhenCaseNumberValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.CaseNumber, Guid.NewGuid().ToString())
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.CaseNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456789012345678")]
    public void ShouldContainErrorWhenNhsNumberInvalid(string? nhsNumber)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.NhsNumber, nhsNumber)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.NhsNumber);
    }

    [Fact]
    public void ShouldNotContainErrorWhenNhsNumberValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.NhsNumber, "12345678901234567")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.NhsNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-date")]
    public void ShouldContainErrorWhenCreationDateInvalid(string? creationDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.CreationDate, creationDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.CreationDate);
    }

    [Theory]
    [InlineData("2024-02-19")]
    [InlineData("2025-03-05T09:14:12.8371094Z")]
    public void ShouldNotContainErrorWhenCreationDateValid(string creationDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.CreationDate, creationDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.CreationDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    public void ShouldContainErrorWhenWaitingListInvalid(string? waitingList)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.WaitingList, waitingList)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.WaitingList);
    }

    [Fact]
    public void ShouldNotContainErrorWhenWaitingListValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.WaitingList, "12")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.WaitingList);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    public void ShouldContainErrorWhenIntendedManagementInvalid(string? intendedManagement)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.IntendedManagement, intendedManagement)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.IntendedManagement);
    }

    [Fact]
    public void ShouldNotContainErrorWhenIntendedManagementValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.IntendedManagement, "1")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.IntendedManagement);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456789")]
    public void ShouldContainErrorWhenReferrerInvalid(string? referrer)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.Referrer, referrer)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Referrer);
    }

    [Fact]
    public void ShouldNotContainErrorWhenReferrerValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.Referrer, "12345678")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.Referrer);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567")]
    public void ShouldContainErrorWhenReferrerAddressInvalid(string? referrerAddress)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferrerAddress, referrerAddress)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.ReferrerAddress);
    }

    [Fact]
    public void ShouldNotContainErrorWhenReferrerAddressValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferrerAddress, "123456")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.ReferrerAddress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456789")]
    public void ShouldContainErrorWhenPatientGpCodeInvalid(string? patientGpCode)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientGpCode, patientGpCode)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.PatientGpCode);
    }

    [Fact]
    public void ShouldNotContainErrorWhenPatientGpCodeValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientGpCode, "12345678")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.PatientGpCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567")]
    public void ShouldContainErrorWhenPatientGpPracticeCodeInvalid(string? patientGpPracticeCode)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientGpPracticeCode, patientGpPracticeCode)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.PatientGpPracticeCode);
    }

    [Fact]
    public void ShouldNotContainErrorWhenPatientGpPracticeCodeValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientGpPracticeCode, "123456")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.PatientGpPracticeCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456789")]
    public void ShouldContainErrorWhenPatientPostcodeInvalid(string? patientPostcode)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientPostcode, patientPostcode)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.PatientPostcode);
    }

    [Fact]
    public void ShouldNotContainErrorWhenPatientPostcodeValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientPostcode, "12345678")
            .Create();

        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.PatientPostcode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")]
    public void ShouldContainErrorWhenPatientHealthBoardAreaCodeInvalid(string? patientHealthBoardAreaCode)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientHealthBoardAreaCode, patientHealthBoardAreaCode)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.PatientHealthBoardAreaCode);
    }

    [Fact]
    public void ShouldNotContainErrorWhenPatientHealthBoardAreaCodeValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientHealthBoardAreaCode, "123")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.PatientHealthBoardAreaCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")]
    public void ShouldContainErrorWhenReferrerSourceTypeInvalid(string? referrerSourceType)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferrerSourceType, referrerSourceType)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.ReferrerSourceType);
    }

    [Fact]
    public void ShouldNotContainErrorWhenReferrerSourceTypeValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferrerSourceType, "12")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.ReferrerSourceType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    public void ShouldContainErrorWhenLetterPriorityInvalid(string? letterPriority)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.LetterPriority, letterPriority)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.LetterPriority);
    }

    [Fact]
    public void ShouldNotContainErrorWhenLetterPriorityValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.LetterPriority, "1")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.LetterPriority);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-date")]
    public void ShouldContainErrorWhenHealthBoardReceiveDateInvalid(string? healthBoardReceiveDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.HealthBoardReceiveDate, healthBoardReceiveDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.HealthBoardReceiveDate);
    }

    [Theory]
    [InlineData("2024-02-19")]
    [InlineData("2025-03-05T09:14:12.8371094Z")]
    public void ShouldNotContainErrorWhenHealthBoardReceiveDateValid(string healthBoardReceiveDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.HealthBoardReceiveDate, healthBoardReceiveDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.HealthBoardReceiveDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456")]
    public void ShouldContainErrorWhenReferralAssignedConsultantInvalid(string? referralAssignedConsultant)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferralAssignedConsultant, referralAssignedConsultant)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.ReferralAssignedConsultant);
    }

    [Fact]
    public void ShouldNotContainErrorWhenReferralAssignedConsultantValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferralAssignedConsultant, "12345")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.ReferralAssignedConsultant);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456")]
    public void ShouldContainErrorWhenReferralAssignedLocationInvalid(string? referralAssignedLocation)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferralAssignedLocation, referralAssignedLocation)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.ReferralAssignedLocation);
    }

    [Fact]
    public void ShouldNotContainErrorWhenReferralAssignedLocationValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferralAssignedLocation, "12345")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.ReferralAssignedLocation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    public void ShouldContainErrorWhenPatientCategoryInvalid(string? patientCategory)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientCategory, patientCategory)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.PatientCategory);
    }

    [Fact]
    public void ShouldNotContainErrorWhenPatientCategoryValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.PatientCategory, "12")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.PatientCategory);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    public void ShouldContainErrorWhenPriorityInvalid(string? priority)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.Priority, priority)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void ShouldNotContainErrorWhenPriorityValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.Priority, "1")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-date")]
    public void ShouldContainErrorWhenBookingDateInvalid(string? bookingDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.BookingDate, bookingDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.BookingDate);
    }

    [Theory]
    [InlineData("2024-02-19")]
    [InlineData("2025-03-05T09:14:12.8371094Z")]
    public void ShouldNotContainErrorWhenBookingDateValid(string bookingDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.BookingDate, bookingDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.BookingDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-date")]
    public void ShouldContainErrorWhenTreatmentDateInvalid(string? treatmentDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.TreatmentDate, treatmentDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.TreatmentDate);
    }

    [Theory]
    [InlineData("2024-02-19")]
    [InlineData("2025-03-05T09:14:12.8371094Z")]
    public void ShouldNotContainErrorWhenTreatmentDateValid(string treatmentDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.TreatmentDate, treatmentDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.TreatmentDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456789012345678901")]
    public void ShouldContainErrorWhenSpecialityIdentifierInvalid(string? specialityIdentifier)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.SpecialityIdentifier, specialityIdentifier)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.SpecialityIdentifier);
    }

    [Fact]
    public void ShouldNotContainErrorWhenSpecialityIdentifierValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.SpecialityIdentifier, "12345678901234567890")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.SpecialityIdentifier);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")]
    public void ShouldContainErrorWhenRepeatPeriodInvalid(string? repeatPeriod)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.RepeatPeriod, repeatPeriod)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.RepeatPeriod);
    }

    [Fact]
    public void ShouldNotContainErrorWhenRepeatPeriodValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.RepeatPeriod, "123")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.RepeatPeriod);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-date")]
    public void ShouldContainErrorWhenFirstAppointmentDateInvalid(string? firstAppointmentDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.FirstAppointmentDate, firstAppointmentDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.FirstAppointmentDate);
    }

    [Theory]
    [InlineData("2024-02-19")]
    [InlineData("2025-03-05T09:14:12.8371094Z")]
    public void ShouldNotContainErrorWhenFirstAppointmentDateValid(string firstAppointmentDate)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.FirstAppointmentDate, firstAppointmentDate)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.FirstAppointmentDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    public void ShouldContainErrorWhenHealthRiskFactorInvalid(string? healthRiskFactor)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.HealthRiskFactor, healthRiskFactor)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.HealthRiskFactor);
    }

    [Fact]
    public void ShouldNotContainErrorWhenHealthRiskFactorValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.HealthRiskFactor, "12")
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.HealthRiskFactor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    public void ShouldContainErrorWhenReferralIdInvalid(string? referralId)
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferralId, referralId)
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.ReferralId);
    }

    [Fact]
    public void ShouldNotContainErrorWhenReferralIdValid()
    {
        //Arrange
        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferralId, Guid.NewGuid().ToString())
            .Create();

        //Act
        var validationResult = _sut.TestValidate(dbModel);

        //Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.ReferralId);
    }
}
