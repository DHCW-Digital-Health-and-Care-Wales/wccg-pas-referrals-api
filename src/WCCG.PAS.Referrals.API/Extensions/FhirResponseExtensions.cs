using Hl7.Fhir.Model;
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
        appointment.Created = dbModel.BookingDate;
    }

    private static void CreateOrUpdateCaseNumber(Patient patient, string caseNumberValue)
    {
        var pasIdentifierSystem = patient.Identifier.SelectWithCondition(x => x.System, NhsFhirConstants.PasIdentifierSystem);

        if (pasIdentifierSystem is null)
        {
            patient.Identifier.Add(new Identifier(NhsFhirConstants.PasIdentifierSystem, caseNumberValue));
            return;
        }

        pasIdentifierSystem.Value = caseNumberValue;
    }

    private static void CreateOrUpdateReferralId(ServiceRequest serviceRequest, string referralIdValue)
    {
        var referralIdSystem = serviceRequest.Identifier.SelectWithCondition(x => x.System, NhsFhirConstants.ReferralIdSystem);

        if (referralIdSystem is null)
        {
            serviceRequest.Identifier.Add(new Identifier(NhsFhirConstants.ReferralIdSystem, referralIdValue));
            return;
        }

        referralIdSystem.Value = referralIdValue;
    }
}
