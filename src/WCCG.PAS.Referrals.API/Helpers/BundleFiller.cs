using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Extensions;

namespace WCCG.PAS.Referrals.API.Helpers;

public class BundleFiller : IBundleFiller
{
    private Bundle? _bundle;
    private ReferralDbModel? _dbModel;

    public void AdjustBundleWithDbModelData(Bundle bundle, ReferralDbModel dbModel)
    {
        _bundle = bundle;
        _dbModel = dbModel;

        SetCaseNumber();
        SetBookingDate();
        SetReferralId();
    }

    private void SetCaseNumber()
    {
        var serviceRequest = _bundle?.ResourceByType<ServiceRequest>()!;
        var patientReference = serviceRequest.Subject.Reference;
        var patient = _bundle?.ResourceByUrl(patientReference) as Patient;

        patient!.Identifier
            .SelectWithCondition(x => x.System, NhsFhirConstants.PasIdentifierSystem)
            !.Value = _dbModel?.CaseNumber;
    }

    private void SetBookingDate()
    {
        var serviceRequest = _bundle?.ResourceByType<ServiceRequest>()!;

        var encounterReference = serviceRequest.Encounter?.Reference;
        var encounter = _bundle?.ResourceByUrl(encounterReference) as Encounter;

        var appointmentReference = encounter!.Appointment.FirstOrDefault()!.Reference;
        var appointment = _bundle?.ResourceByUrl(appointmentReference) as Appointment;

        appointment!.Created = _dbModel!.BookingDate;
    }

    private void SetReferralId()
    {
        var serviceRequest = _bundle?.ResourceByType<ServiceRequest>()!;

        serviceRequest.Identifier.SelectWithCondition(x => x.System, NhsFhirConstants.ReferralIdSystem)
            !.Value = _dbModel!.ReferralId;
    }
}
