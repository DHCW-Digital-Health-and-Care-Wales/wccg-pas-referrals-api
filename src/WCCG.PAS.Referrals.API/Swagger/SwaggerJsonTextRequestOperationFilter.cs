using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WCCG.PAS.Referrals.API.Swagger;

public class SwaggerJsonTextRequestOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var rawTextRequestAttribute = context.MethodInfo.GetCustomAttribute<SwaggerJsonRequestAttribute>();

        if (rawTextRequestAttribute is null)
        {
            return;
        }

        operation.RequestBody = new OpenApiRequestBody();
        operation.RequestBody.Content.Add(rawTextRequestAttribute.MediaType,
            new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } });
    }
}
