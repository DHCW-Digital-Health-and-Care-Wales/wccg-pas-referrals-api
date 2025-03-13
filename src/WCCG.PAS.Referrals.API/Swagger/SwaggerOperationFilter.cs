using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WCCG.PAS.Referrals.API.Constants;

namespace WCCG.PAS.Referrals.API.Swagger;

[ExcludeFromCodeCoverage]
public class SwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        HandleCreateReferral(operation, context);
        HandleGetReferral(operation, context);
    }

    private static void HandleCreateReferral(OpenApiOperation operation, OperationFilterContext context)
    {
        var createReferralRequestAttribute = context.MethodInfo.GetCustomAttribute<SwaggerCreateReferralRequestAttribute>();

        if (createReferralRequestAttribute is null)
        {
            return;
        }

        operation.Parameters = [];

        AddCreateReferralResponses(operation);
        AddCreateReferralRequests(operation);
    }

    private static void HandleGetReferral(OpenApiOperation operation, OperationFilterContext context)
    {
        var getReferralRequestAttribute = context.MethodInfo.GetCustomAttribute<SwaggerGetReferralRequestAttribute>();

        if (getReferralRequestAttribute is null)
        {
            return;
        }

        operation.Parameters =
        [
            new OpenApiParameter
            {
                In = ParameterLocation.Path,
                Name = "referralId",
                Required = true,
                Example = new OpenApiString(Guid.NewGuid().ToString())
            }
        ];

        AddGetReferralResponses(operation);
    }

    private static void AddCreateReferralRequests(OpenApiOperation operation)
    {
        operation.RequestBody = new OpenApiRequestBody();
        operation.RequestBody.Content.Add(FhirConstants.FhirMediaType,
            new OpenApiMediaType
            {
                Example = new OpenApiString(
                    File.ReadAllText("Swagger/Examples/create-referral-payload&response.json"))
            });
    }

    private static void AddGetReferralResponses(OpenApiOperation operation)
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
                                Example = new OpenApiString(File.ReadAllText("Swagger/Examples/get-referral-bad-request.json")),
                            }
                        }
                    }
                }
            },
            {
                "404", new OpenApiResponse
                {
                    Description = "Not Found",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            MediaTypeNames.Application.Json, new OpenApiMediaType
                            {
                                Example = new OpenApiString(File.ReadAllText("Swagger/Examples/get-referral-not-found.json")),
                            }
                        }
                    }
                }
            },
            {
                "429", new OpenApiResponse
                {
                    Description = "Too Many Requests",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            MediaTypeNames.Application.Json, new OpenApiMediaType
                            {
                                Example = new OpenApiString(File.ReadAllText("Swagger/Examples/common-too-many-requests.json")),
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

    private static void AddCreateReferralResponses(OpenApiOperation operation)
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
                "429", new OpenApiResponse
                {
                    Description = "Too Many Requests",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            MediaTypeNames.Application.Json, new OpenApiMediaType
                            {
                                Example = new OpenApiString(File.ReadAllText("Swagger/Examples/common-too-many-requests.json")),
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
