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
            ?.SelectWithCondition(x => x.System, NhsFhirConstants.NhsNumberSystem)
            ?.Value;
    }

    private string? GetReferrer()
    {
        var practitionerReference = ServiceRequest?.Requester?.Reference;
        var practitioner = _bundle?.GetResourceByUrl(practitionerReference) as Practitioner;

        return practitioner.CheckPropertyValue(x => x?.Id, NhsFhirConstants.RequestingPractitioner)
            ?.Identifier.SelectWithCondition(x => x.System, NhsFhirConstants.GmcNumberSystem)
            ?.Value;
    }

    private string? GetReferrerAddress()
    {
        var appointmentReference = EncounterFromServiceRequest?.Appointment?.FirstOrDefault()?.Reference;
        var appointment = _bundle?.GetResourceByUrl(appointmentReference) as Appointment;

        var organisationReference = appointment?.Extension?.SelectWithCondition(x => x.Url, NhsFhirConstants.BookingOrganisation)
            ?.Value.NamedChildren?.GetStringValueByElementName(NhsFhirConstants.ReferenceElementName);
        var organisation = _bundle?.GetResourceByUrl(organisationReference) as Organization;

        return organisation?.Identifier?.SelectWithCondition(x => x.System, NhsFhirConstants.OdcOrganizationCodeSystem)
            ?.Value;
    }

    private string? GetPatientGpCode()
    {
        return PatientFromServiceRequest?.GeneralPractitioner?.SelectWithCondition(x => x.Type, NhsFhirConstants.PractitionerType)
            ?.Identifier?.CheckPropertyValue(x => x?.System, NhsFhirConstants.GmcNumberSystem)
            ?.Value;
    }

    private string? GetPatientGpPracticeCode()
    {
        return PatientFromServiceRequest?.GeneralPractitioner?.SelectWithCondition(x => x.Type, NhsFhirConstants.OrganizationType)
            ?.Identifier?.CheckPropertyValue(x => x?.System, NhsFhirConstants.OdcOrganizationCodeSystem)
            ?.Value;
    }

    private string? GetPostcode()
    {
        return PatientFromServiceRequest?.Address?.FirstOrDefault()?.PostalCode;
    }

    private string? GetPatientHealthBoardAreaCode()
    {
        var organization = ServiceRequest?.Performer?.GetResourceByIdFromList(_bundle, NhsFhirConstants.DhaCodeId) as Organization;

        return organization?.Identifier?.SelectWithCondition(x => x.System, NhsFhirConstants.OdcOrganizationCodeSystem)
            ?.Value;
    }

    private string? GetLetterPriority()
    {
        return EncounterFromServiceRequest?.Priority?.Coding
            ?.SelectWithCondition(x => x.System, NhsFhirConstants.LetterPrioritySystem)
            ?.Code;
    }

    private string? GetReferralAssignedConsultant()
    {
        var practitioner =
            ServiceRequest?.Performer?.GetResourceByIdFromList(_bundle, NhsFhirConstants.ReceivingClinicianId) as Practitioner;

        return practitioner?.Identifier?.SelectWithCondition(x => x.System, NhsFhirConstants.GdcNumberSystem)
            ?.Value;
    }

    private string? GetReferralAssignedLocation()
    {
        return ServiceRequest?.LocationCode?.SelectNestedWithCondition(
                x => x?.Coding,
                c => c?.System,
                NhsFhirConstants.OdcOrganizationCodeSystem)
            ?.Code;
    }

    private string? GetPatientCategory()
    {
        return ServiceRequest?.OrderDetail?.SelectNestedWithCondition(
                x => x?.Coding,
                c => c?.System,
                NhsFhirConstants.PatientCategorySystem)
            ?.Code;
    }

    private string? GetSpecialityIdentifier()
    {
        return EncounterFromServiceRequest?.ServiceType?.Coding
            ?.SelectWithCondition(x => x.System, NhsFhirConstants.SpecialityIdentifierSystem)
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
                NhsFhirConstants.RiskFactorSystem)
            ?.Code;
    }

    private ServiceRequest? GetServiceRequest()
    {
        return _bundle?.GetResourceByType<ServiceRequest>();
    }

    private Patient? GetPatient()
    {
        var patientReference = ServiceRequest?.Subject?.Reference;
        return _bundle?.GetResourceByUrl(patientReference) as Patient;
    }

    private Encounter? GetEncounter()
    {
        var encounterReference = ServiceRequest?.Encounter?.Reference;
        return _bundle?.GetResourceByUrl(encounterReference) as Encounter;
    }
}
