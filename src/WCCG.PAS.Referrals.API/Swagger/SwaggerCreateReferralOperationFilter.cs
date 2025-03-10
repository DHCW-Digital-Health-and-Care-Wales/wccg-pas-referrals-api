using System.Net.Mime;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WCCG.PAS.Referrals.API.Constants;

namespace WCCG.PAS.Referrals.API.Swagger;

public class SwaggerCreateReferralOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var processMessageRequestAttribute = context.MethodInfo.GetCustomAttribute<SwaggerCreateReferralRequestAttribute>();

        if (processMessageRequestAttribute is null)
        {
            return;
        }

        operation.Parameters = [];

        AddResponses(operation);
        AddRequests(operation);
    }

    private static void AddRequests(OpenApiOperation operation)
    {
        operation.RequestBody = new OpenApiRequestBody();
        operation.RequestBody.Content.Add(FhirConstants.FhirMediaType,
            new OpenApiMediaType
            {
                Example = new OpenApiString(
                    File.ReadAllText("Swagger/Examples/create-referral-payload&response.json"))
            });
    }

    private static void AddResponses(OpenApiOperation operation)
    {
        operation.Responses = new OpenApiResponses
        {
            {
                "200", new OpenApiResponse
                {
                    Description = "OK",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            FhirConstants.FhirMediaType, new OpenApiMediaType
                            {
                                Example = new OpenApiString(
                                    File.ReadAllText("Swagger/Examples/create-referral-payload&response.json")),
                            }
                        }
                    }
                }
            },
            {
                "400", new OpenApiResponse
                {
                    Description = "Bad Request",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            MediaTypeNames.Application.Json, new OpenApiMediaType
                            {
                                Example = new OpenApiString(File.ReadAllText("Swagger/Examples/create-referral-bad-request.json")),
                            }
                        }
                    }
                }
            },
            {
                "500", new OpenApiResponse
                {
                    Description = "Internal Server Error",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            MediaTypeNames.Application.Json, new OpenApiMediaType
                            {
                                Example = new OpenApiString(File.ReadAllText("Swagger/Examples/create-referral-internal-error.json")),
                            }
                        }
                    }
                }
            }
        };
    }
}
