using FluentValidation;
using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Validators;

public class ReferralDbModelValidator : AbstractValidator<ReferralDbModel>
{
    public ReferralDbModelValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(BeValidGuid);

        RuleFor(x => x.CaseNumber)
            .NotEmpty()
            .Must(BeValidGuid);

        RuleFor(x => x.NhsNumber)
            .NotEmpty()
            .MaximumLength(17);

        RuleFor(x => x.CreationDate)
            .NotEmpty()
            .Must(BeValidDate);

        RuleFor(x => x.WaitingList)
            .NotEmpty()
            .MaximumLength(2);

        RuleFor(x => x.IntendedManagement)
            .NotEmpty()
            .MaximumLength(1);

        RuleFor(x => x.Referrer)
            .NotEmpty()
            .MaximumLength(8);

        RuleFor(x => x.ReferrerAddress)
            .NotEmpty()
            .MaximumLength(6);

        RuleFor(x => x.PatientGpCode)
            .NotEmpty()
            .MaximumLength(8);

        RuleFor(x => x.PatientGpPracticeCode)
            .NotEmpty()
            .MaximumLength(6);

        RuleFor(x => x.PatientPostcode)
            .NotEmpty()
            .MaximumLength(8);

        RuleFor(x => x.PatientHealthBoardAreaCode)
            .NotEmpty()
            .MaximumLength(3);

        RuleFor(x => x.ReferrerSourceType)
            .NotEmpty()
            .MaximumLength(2);

        RuleFor(x => x.LetterPriority)
            .NotEmpty()
            .MaximumLength(1);

        RuleFor(x => x.HealthBoardReceiveDate)
            .NotEmpty()
            .Must(BeValidDate);

        RuleFor(x => x.ReferralAssignedConsultant)
            .NotEmpty()
            .MaximumLength(5);

        RuleFor(x => x.ReferralAssignedLocation)
            .NotEmpty()
            .MaximumLength(5);

        RuleFor(x => x.PatientCategory)
            .NotEmpty()
            .MaximumLength(2);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .MaximumLength(1);

        RuleFor(x => x.BookingDate)
            .NotEmpty()
            .Must(BeValidDate);

        RuleFor(x => x.TreatmentDate)
            .NotEmpty()
            .Must(BeValidDate);

        RuleFor(x => x.SpecialityIdentifier)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.RepeatPeriod)
            .NotEmpty()
            .MaximumLength(3);

        RuleFor(x => x.FirstAppointmentDate)
            .NotEmpty()
            .Must(BeValidDate);

        RuleFor(x => x.HealthRiskFactor)
            .NotEmpty()
            .MaximumLength(2);

        RuleFor(x => x.ReferralId)
            .NotEmpty()
            .Must(BeValidGuid);
    }

    private static bool BeValidGuid(string? value)
    {
        return Guid.TryParse(value, out _);
    }

    private static bool BeValidDate(string? value)
    {
        return FhirDateTime.IsValidValue(value!);
    }
}
