using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Cosmos;
using WCCG.PAS.Referrals.API.Helpers;

namespace WCCG.PAS.Referrals.API.Handlers;

[ExcludeFromCodeCoverage]
public class AppInsightsRequestHandler : RequestHandler
{
    private readonly TelemetryClient _telemetryClient;

    public AppInsightsRequestHandler(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public override async Task<ResponseMessage> SendAsync(RequestMessage request, CancellationToken cancellationToken)
    {
        using var dependency = _telemetryClient.StartOperation<DependencyTelemetry>("Cosmos");

        var response = await base.SendAsync(request, cancellationToken);

        var telemetry = dependency.Telemetry;
        var resourcePath = HttpParsingHelper.ParseResourcePath(request.RequestUri.OriginalString);

        foreach (var (key, value) in resourcePath)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (value is not null)
            {
                var propertyName = GetPropertyNameForResource(key);
                if (propertyName is not null)
                {
                    telemetry.Properties[propertyName] = value;
                }
            }
        }

        var operation = HttpParsingHelper.BuildOperationMoniker(request.Method.ToString(), resourcePath);
        var operationName = GetOperationName(operation);

        telemetry.Type = "Azure DocumentDB";
        telemetry.Name = operationName;
        telemetry.Data = request.RequestUri.OriginalString;

        telemetry.ResultCode = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);
        telemetry.Success = response.IsSuccessStatusCode;

        telemetry.Metrics["RequestCharge"] = response.Headers.RequestCharge;

        return response;
    }

    private static readonly Dictionary<string, string> OperationNames = new()
    {
        // Database operations
        ["POST /dbs"] = "Create database",
        ["GET /dbs"] = "List databases",
        ["GET /dbs/*"] = "Get database",
        ["DELETE /dbs/*"] = "Delete database",

        // Collection operations
        ["POST /dbs/*/colls"] = "Create collection",
        ["GET /dbs/*/colls"] = "List collections",
        ["POST /dbs/*/colls/*"] = "Query documents",
        ["GET /dbs/*/colls/*"] = "Get collection",
        ["DELETE /dbs/*/colls/*"] = "Delete collection",
        ["PUT /dbs/*/colls/*"] = "Replace collection",

        // Document operations
        ["POST /dbs/*/colls/*/docs"] = "Create document",
        ["GET /dbs/*/colls/*/docs"] = "List documents",
        ["GET /dbs/*/colls/*/docs/*"] = "Get document",
        ["PUT /dbs/*/colls/*/docs/*"] = "Replace document",
        ["DELETE /dbs/*/colls/*/docs/*"] = "Delete document"
    };

    private static string? GetPropertyNameForResource(string resourceType)
    {
        // ignore high cardinality resources (documents, attachments, etc.)
        return resourceType switch
        {
            "dbs" => "Database",
            "colls" => "Collection",
            _ => null
        };
    }

    private static string GetOperationName(string operation)
    {
        return OperationNames.GetValueOrDefault(operation, operation);
    }
}
