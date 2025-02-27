using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Hl7.Fhir.Serialization;
using Microsoft.Azure.Cosmos;

namespace WCCG.PAS.Referrals.API.Extensions;

[ExcludeFromCodeCoverage]
public static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Called {methodName}.")]
    public static partial void CalledMethod(this ILogger logger, string methodName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to deserialize bundle.")]
    public static partial void BundleDeserializationFailure(this ILogger logger, DeserializationFailedException exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Cosmos database failure.")]
    public static partial void CosmosDatabaseFailure(this ILogger logger, CosmosException exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unexpected error.")]
    public static partial void UnexpectedError(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "ReferralDbModel validation failed: {validationErrors}.")]
    public static partial void ReferralValidationFailed(this ILogger logger, string validationErrors);

    [LoggerMessage(Level = LogLevel.Error, Message = "Invalid JSON.")]
    public static partial void InvalidJson(this ILogger logger, JsonException exception);
}
