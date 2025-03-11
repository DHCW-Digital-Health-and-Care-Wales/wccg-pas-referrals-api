using System.Net;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using WCCG.PAS.Referrals.API.Extensions;

namespace WCCG.PAS.Referrals.API.Middleware;

public class ResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseMiddleware> _logger;

    private readonly Dictionary<HttpStatusCode, string> _cosmosErrorDictionary = new()
    {
        { HttpStatusCode.NotFound, "Referral with the provided ID was not found." },
        { HttpStatusCode.Forbidden, "Access denied: Unable to connect to CosmosDB." },
        { HttpStatusCode.BadRequest, "Bad request while calling CosmosDB." },
        { HttpStatusCode.InternalServerError, "Unexpected error occurred while calling CosmosDB." }
    };

    public ResponseMiddleware(RequestDelegate next, ILogger<ResponseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;

        var statusCode = HttpStatusCode.InternalServerError;
        var body = new ProblemDetails();
        switch (exception)
        {
            case DeserializationFailedException deserializationFailed:
                _logger.BundleDeserializationFailure(deserializationFailed);

                statusCode = HttpStatusCode.BadRequest;
                body.Title = "Failed to deserialize bundle";
                body.Detail = deserializationFailed.Message;
                break;

            case JsonException jsonException:
                _logger.InvalidJson(jsonException);

                statusCode = HttpStatusCode.BadRequest;
                body.Title = "Invalid JSON";
                body.Detail = jsonException.Message;
                break;

            case ValidationException validationException:
                var errorMessages = validationException.Errors.Select(x => x.ErrorMessage);
                _logger.ReferralValidationFailed(string.Join(';', errorMessages));

                statusCode = HttpStatusCode.BadRequest;
                body.Title = "Validation Failed";
                body.Extensions = new Dictionary<string, object?>
                {
                    {
                        "validationErrors", validationException.Errors.Select(e => new
                        {
                            e.PropertyName,
                            e.ErrorMessage
                        })
                    }
                };
                break;

            case CosmosException cosmosException:
                _logger.CosmosDatabaseFailure(cosmosException);
                statusCode = cosmosException.StatusCode;
                body.Title = "Cosmos database failure";
                body.Detail = _cosmosErrorDictionary[statusCode];
                break;

            default:
                _logger.UnexpectedError(exception);
                body.Title = "Unexpected error";
                body.Detail = exception.Message;
                break;
        }

        response.Headers.ContentType = "application/json";
        response.StatusCode = (int)statusCode;
        await response.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body)));
    }
}
