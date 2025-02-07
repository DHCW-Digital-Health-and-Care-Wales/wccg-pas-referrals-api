using Newtonsoft.Json;

namespace WCCG.PAS.Referrals.UI.Models;

public class Referral
{
    [JsonRequired]
    public string Id { get; set; }

    [JsonRequired]
    public string CaseNumber { get; set; }
}
