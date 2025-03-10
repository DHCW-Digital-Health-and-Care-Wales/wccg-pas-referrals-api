using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Extensions;

namespace WCCG.PAS.Referrals.API.Mappers;

public class ReferralMapper : IReferralMapper
{
    private Bundle? _bundle;

    private ServiceRequest? _serviceRequest;
    private ServiceRequest? ServiceRequest { get { return _serviceRequest ??= GetServiceRequest(); } }

    private Patient? _patientFromServiceRequest;
    private Patient? PatientFromServiceRequest { get { return _patientFromServiceRequest ??= GetPatient(); } }

    private Encounter? _encounterFromServiceRequest;
    private Encounter? EncounterFromServiceRequest { get { return _encounterFromServiceRequest ??= GetEncounter(); } }

    public ReferralDbModel MapFromBundle(Bundle bundle)
    {
        _bundle = bundle;

        var currentDate = DateTimeOffset.UtcNow.ToString("O");
        return new ReferralDbModel
        {
            CaseNumber = Guid.NewGuid().ToString(),
            NhsNumber = GetNhsNumber(),
            CreationDate = ServiceRequest?.AuthoredOn,
            WaitingList = "O",
            IntendedManagement = "6",
            Referrer = GetReferrer(),
            ReferrerAddress = GetReferrerAddress(),
            PatientGpCode = GetPatientGpCode(),
            PatientGpPracticeCode = GetPatientGpPracticeCode(),
            PatientPostcode = GetPostcode(),
            PatientHealthBoardAreaCode = GetPatientHealthBoardAreaCode(),
            ReferrerSourceType = "DE",
            LetterPriority = GetLetterPriority(),
            HealthBoardReceiveDate = currentDate,
            ReferralAssignedConsultant = GetReferralAssignedConsultant(),
            ReferralAssignedLocation = GetReferralAssignedLocation(),
            PatientCategory = GetPatientCategory(),
            Priority = ServiceRequest?.Priority.ToString()?[..1],
            BookingDate = currentDate,
            TreatmentDate = currentDate,
            SpecialityIdentifier = GetSpecialityIdentifier(),
            RepeatPeriod = GetRepeatPeriod(),
            FirstAppointmentDate = GetFirstAppointmentDate(),
            HealthRiskFactor = GetHealthRiskFactor(),
            ReferralId = Guid.NewGuid().ToString()
        };
    }

    private string? GetNhsNumber()
    {
        return PatientFromServiceRequest?.Identifier
            ?.SelectWithCondition(x => x.System, FhirConstants.NhsNumberSystem)
            ?.Value;
    }

    private string? GetReferrer()
    {
        var practitionerReference = ServiceRequest?.Requester?.Reference;
        var practitioner =
            _bundle?.GetResourceByUrlWithCondition<Practitioner>(practitionerReference, p => p.Id, FhirConstants.RequestingPractitioner);

        return practitioner?.Identifier.SelectWithCondition(x => x.System, FhirConstants.GmcNumberSystem)
            ?.Value;
    }

    private string? GetReferrerAddress()
    {
        var appointmentReference = EncounterFromServiceRequest?.Appointment?.FirstOrDefault()?.Reference;
        var appointment = _bundle?.GetResourceByUrl<Appointment>(appointmentReference);

        var organisationReference = appointment?.Extension?.SelectWithCondition(x => x.Url, FhirConstants.BookingOrganisation)
            ?.Value.NamedChildren?.GetStringValueByElementName(FhirConstants.ReferenceElementName);
        var organisation = _bundle?.GetResourceByUrl<Organization>(organisationReference);

        return organisation?.Identifier?.SelectWithCondition(x => x.System, FhirConstants.OdcOrganizationCodeSystem)
            ?.Value;
    }

    private string? GetPatientGpCode()
    {
        return PatientFromServiceRequest?.GeneralPractitioner
            ?.SelectWithConditions(
                (x => x.Type, FhirConstants.PractitionerType),
                (x => x.Identifier?.System, FhirConstants.GmcNumberSystem)
            )?.Identifier.Value;
    }

    private string? GetPatientGpPracticeCode()
    {
        return PatientFromServiceRequest?.GeneralPractitioner?.SelectWithConditions(
            (x => x.Type, FhirConstants.OrganizationType),
            (x => x.Identifier?.System, FhirConstants.OdcOrganizationCodeSystem)
        )?.Identifier.Value;
    }

    private string? GetPostcode()
    {
        return PatientFromServiceRequest?.Address?.FirstOrDefault()?.PostalCode;
    }

    private string? GetPatientHealthBoardAreaCode()
    {
        var organization = ServiceRequest?.Performer?.GetResourceByIdFromList<Organization>(_bundle, FhirConstants.DhaCodeId);

        return organization?.Identifier?.SelectWithCondition(x => x.System, FhirConstants.OdcOrganizationCodeSystem)
            ?.Value;
    }

    private string? GetLetterPriority()
    {
        return EncounterFromServiceRequest?.Priority?.Coding
            ?.SelectWithCondition(x => x.System, FhirConstants.LetterPrioritySystem)
            ?.Code;
    }

    private string? GetReferralAssignedConsultant()
    {
        var practitioner =
            ServiceRequest?.Performer?.GetResourceByIdFromList<Practitioner>(_bundle, FhirConstants.ReceivingClinicianId);

        return practitioner?.Identifier?.SelectWithCondition(x => x.System, FhirConstants.GdcNumberSystem)
            ?.Value;
    }

    private string? GetReferralAssignedLocation()
    {
        return ServiceRequest?.LocationCode?.SelectNestedWithCondition(
                x => x?.Coding,
                c => c?.System,
                FhirConstants.OdcOrganizationCodeSystem)
            ?.Code;
    }

    private string? GetPatientCategory()
    {
        return ServiceRequest?.OrderDetail?.SelectNestedWithCondition(
                x => x?.Coding,
                c => c?.System,
                FhirConstants.PatientCategorySystem)
            ?.Code;
    }

    private string? GetSpecialityIdentifier()
    {
        return EncounterFromServiceRequest?.ServiceType?.Coding
            ?.SelectWithCondition(x => x.System, FhirConstants.SpecialityIdentifierSystem)
            ?.Code;
    }

    private string? GetRepeatPeriod()
    {
        var occurrence = ServiceRequest?.Occurrence as Timing;

        return occurrence?.Repeat?.Period is not null && occurrence.Repeat?.PeriodUnit is not null
            ? $"{occurrence.Repeat.Period}{occurrence.Repeat.PeriodUnit}"
            : null;
    }

    private string? GetFirstAppointmentDate()
    {
        var occurrence = ServiceRequest?.Occurrence as Timing;
        return occurrence?.Event.FirstOrDefault();
    }

    private string? GetHealthRiskFactor()
    {
        return ServiceRequest?.OrderDetail?.SelectNestedWithCondition(
                x => x?.Coding,
                c => c?.System,
                FhirConstants.RiskFactorSystem)
            ?.Code;
    }

    private ServiceRequest? GetServiceRequest()
    {
        return _bundle?.GetResourceByType<ServiceRequest>();
    }

    private Patient? GetPatient()
    {
        var patientReference = ServiceRequest?.Subject?.Reference;
        return _bundle?.GetResourceByUrl<Patient>(patientReference);
    }

    private Encounter? GetEncounter()
    {
        var encounterReference = ServiceRequest?.Encounter?.Reference;
        return _bundle?.GetResourceByUrl<Encounter>(encounterReference);
    }
}
