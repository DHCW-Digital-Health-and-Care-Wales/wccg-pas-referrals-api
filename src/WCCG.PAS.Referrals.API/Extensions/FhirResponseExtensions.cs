using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Extensions;

public static class FhirResponseExtensions
{
    public static void EnrichForResponse(this Bundle bundle, ReferralDbModel dbModel)
    {
        var serviceRequest = bundle.GetResourceByType<ServiceRequest>()!;
        var patient = bundle.GetResourceByUrl<Patient>(serviceRequest.Subject.Reference)!;
        var encounter = bundle.GetResourceByUrl<Encounter>(serviceRequest.Encounter.Reference)!;
        var appointment = bundle.GetResourceByUrl<Appointment>(encounter.Appointment.First().Reference)!;

        CreateOrUpdateCaseNumber(patient, dbModel.CaseNumber!);
        CreateOrUpdateReferralId(serviceRequest, dbModel.ReferralId!);
        appointment.Created = PrimitiveTypeConverter.ConvertTo<string>(dbModel.BookingDate!.Value);
    }

    private static void CreateOrUpdateCaseNumber(Patient patient, string caseNumberValue)
    {
        var pasIdentifierSystem = patient.Identifier.SelectWithCondition(x => x.System, FhirConstants.PasIdentifierSystem);

        if (pasIdentifierSystem is null)
        {
            patient.Identifier.Add(new Identifier(FhirConstants.PasIdentifierSystem, caseNumberValue));
            return;
        }

        pasIdentifierSystem.Value = caseNumberValue;
    }

    private static void CreateOrUpdateReferralId(ServiceRequest serviceRequest, string referralIdValue)
    {
        var referralIdSystem = serviceRequest.Identifier.SelectWithCondition(x => x.System, FhirConstants.ReferralIdSystem);

        if (referralIdSystem is null)
        {
            serviceRequest.Identifier.Add(new Identifier(FhirConstants.ReferralIdSystem, referralIdValue));
            return;
        }

        referralIdSystem.Value = referralIdValue;
    }
}
