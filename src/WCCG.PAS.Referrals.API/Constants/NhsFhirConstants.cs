namespace WCCG.PAS.Referrals.API.Constants;

public class NhsFhirConstants
{
    public const string NhsNumberSystem = "https://fhir.nhs.uk/Id/nhs-number";
    public const string PasIdentifierSystem = "https://fhir.hduhb.nhs.wales/Id/pas-identifier";
    public const string GmcNumberSystem = "https://fhir.hl7.org.uk/Id/gmc-number";
    public const string OdcOrganizationCodeSystem = "https://fhir.nhs.uk/Id/ods-organization-code";
    public const string GdcNumberSystem = "https://fhir.hl7.org.uk/Id/gdc-number";

    public const string RequestingPractitioner = "requestingPractitioner";
    public const string BookingOrganisation = "bookingOrganization";
    public const string ReferenceElementName = "reference";
    public const string PractitionerType = "practitioner";
    public const string OrganizationType = "organization";
    public const string DhaCodeId = "DhaCode";
    public const string ReceivingClinicianId = "ReceivingClinician";
    public const string LetterPrioritySystem = "DHCW/lttrPriority";
    public const string PatientCategorySystem = "dhcw/patientCategory";
    public const string SpecialityIdentifierSystem = "dhcw/SPEC";
    public const string RiskFactorSystem = "dhcw/optom/hrf";
    public const string ReferralIdSystem = "ReferralUniqueId";
}
