using System.Net;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Hl7.Fhir.Serialization;
using Microsoft.Azure.Cosmos;
using WCCG.PAS.Referrals.API.Extensions;

namespace WCCG.PAS.Referrals.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        string? body = null;
        switch (exception)
        {
            case DeserializationFailedException deserializationFailed:
                _logger.BundleDeserializationFailure(deserializationFailed);

                statusCode = HttpStatusCode.BadRequest;
                body = deserializationFailed.Message;
                break;

            case JsonException jsonException:
                _logger.BundleDeserializationFailure(jsonException);

                statusCode = HttpStatusCode.BadRequest;
                body = jsonException.Message;
                break;

            case ValidationException validationException:
                var errorMessages = validationException.Errors.Select(x => x.ErrorMessage);
                _logger.ReferralValidationFailed(string.Join(';', errorMessages));

                statusCode = HttpStatusCode.BadRequest;
                body = JsonSerializer.Serialize(validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
                response.Headers.ContentType = "application/json";
                break;

            case CosmosException cosmosException:
                _logger.CosmosDatabaseFailure(cosmosException);

                statusCode = cosmosException.StatusCode;
                body = cosmosException.Message;
                break;

            default:
                _logger.UnexpectedError(exception);
                break;
        }

        response.StatusCode = (int)statusCode;
        if (body is not null)
        {
            await response.Body.WriteAsync(Encoding.UTF8.GetBytes(body));
        }
    }
}
