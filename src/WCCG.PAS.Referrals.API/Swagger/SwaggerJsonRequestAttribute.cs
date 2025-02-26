namespace WCCG.PAS.Referrals.API.Swagger;

[AttributeUsage(AttributeTargets.Method)]
public class SwaggerJsonRequestAttribute : Attribute
{
    public string MediaType => "application/fhir+json";
}
