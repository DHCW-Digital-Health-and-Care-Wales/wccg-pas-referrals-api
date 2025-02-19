namespace WCCG.PAS.Referrals.API.Helpers;

public interface IFhirSerializer
{
    T? Deserialize<T>(string inputString);
    string Serialize<T>(T value);
}
