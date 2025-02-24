namespace WCCG.PAS.Referrals.API.Swagger;

[AttributeUsage(AttributeTargets.Method)]
public class RawTextRequestAttribute : Attribute
{
    public string MediaType => "text/plain";
}
