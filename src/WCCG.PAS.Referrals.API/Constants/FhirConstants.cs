namespace WCCG.PAS.Referrals.API.Constants;

public class FhirConstants
{
    public const string FhirMediaType = "application/fhir+json";

    //System
    public const string NhsNumberSystem = "https://fhir.nhs.uk/Id/nhs-number";
    public const string PasIdentifierSystem = "https://fhir.hduhb.nhs.wales/Id/pas-identifier";
    public const string GmcNumberSystem = "https://fhir.hl7.org.uk/Id/gmc-number";
    public const string OdcOrganizationCodeSystem = "https://fhir.nhs.uk/Id/ods-organization-code";
    public const string GdcNumberSystem = "https://fhir.hl7.org.uk/Id/gdc-number";
    public const string SctSystem = "http://snomed.info/sct";
    public const string EvenBarSystem = "https://fhir.nhs.uk/CodeSystem/message-events-bars";
    public const string ReasonBarSystem = "https://fhir.nhs.uk/CodeSystem/message-reason-bars";
    public const string ServiceRequestCategorySystem = "https://fhir.nhs.uk/CodeSystem/message-category-servicerequest";
    public const string VerificationStatusSystem = "https://fhir.hl7.org.uk/CodeSystem/UKCore-NHSNumberVerificationStatusEngland";
    public const string ReferralIdSystem = "ReferralUniqueId";
    public const string RiskFactorSystem = "dhcw/optom/hrf";
    public const string SpecialityIdentifierSystem = "dhcw/SPEC";
    public const string LetterPrioritySystem = "DHCW/lttrPriority";
    public const string PatientCategorySystem = "dhcw/patientCategory";
    public const string WlistCodesSystem = "dhcw/WlistCodes";
    public const string IntentReferValuesSystem = "dhcw/IntentReferValues";
    public const string DatonsysSystem = "dhcw/Datonsys";

    //Id
    public const string RequestingPractitionerId = "RequestingPractitioner";
    public const string ReferringPracticeId = "ReferringPractice";
    public const string DestinationId = "Destination";
    public const string ReceivingClinicianId = "ReceivingClinician";
    public const string DhaCodeId = "DhaCode";

    //Other
    public const string BookingOrganisation = "BookingOrganization";
    public const string ReferenceElementName = "Reference";
    public const string PractitionerType = "Practitioner";
    public const string OrganizationType = "Organization";
    public const string ServiceId = "0123456789";
}
