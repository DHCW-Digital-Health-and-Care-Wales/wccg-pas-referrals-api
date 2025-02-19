using Newtonsoft.Json;

namespace WCCG.PAS.Referrals.API.DbModels;

public class ReferralDbModel
{
    [JsonProperty("iD")]
    public required string Id { get; set; }

    [JsonProperty("caseno")]
    public required string? CaseNumber { get; set; }

    [JsonProperty("nhs")]
    public required string? NhsNumber { get; set; }

    [JsonProperty("datRef")]
    public required string? CreationDate { get; set; }

    [JsonProperty("wlist")]
    public required string? WaitingList { get; set; }

    [JsonProperty("intentRefer")]
    public required string? IntendedManagement { get; set; }

    [JsonProperty("gpRef")]
    public required string? Referrer { get; set; }

    [JsonProperty("gpPrac")]
    public required string? ReferrerAddress { get; set; }

    [JsonProperty("regGp")]
    public required string? PatientGpCode { get; set; }

    [JsonProperty("regPrac")]
    public required string? PatientGpPracticeCode { get; set; }

    [JsonProperty("postcode")]
    public required string? PatientPostcode { get; set; }

    [JsonProperty("dhaCode")]
    public required string? PatientHealthBoardAreaCode { get; set; }

    [JsonProperty("sourceRefer")]
    public required string? ReferrerSourceType { get; set; }

    [JsonProperty("lttrPrty")]
    public required string? LetterPriority { get; set; }

    [JsonProperty("datonsys")]
    public required string? HealthBoardReceiveDate { get; set; }

    [JsonProperty("cons")]
    public required string? ReferralAssignedConsultant { get; set; }

    [JsonProperty("loc")]
    public required string? ReferralAssignedLocation { get; set; }

    [JsonProperty("category")]
    public required string? PatientCategory { get; set; }

    [JsonProperty("consPrty")]
    public required string? Priority { get; set; }

    [JsonProperty("dateBooked")]
    public required string? BookingDate { get; set; }

    [JsonProperty("trtDate")]
    public required string? TreatmentDate { get; set; }

    [JsonProperty("spec")]
    public required string? SpecialityIdentifier { get; set; }

    [JsonProperty("firstApproxFreq")]
    public required string? RepeatPeriod { get; set; }

    [JsonProperty("firstApproxAppt")]
    public required string? FirstAppointmentDate { get; set; }

    [JsonProperty("healthRiskFactor")]
    public required string? HealthRiskFactor { get; set; }

    [JsonProperty("uniqueReferralId")]
    public required string? ReferralId { get; set; }
}
