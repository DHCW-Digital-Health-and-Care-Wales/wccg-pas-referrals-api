using System.Net;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using WCCG.PAS.Referrals.API.Middleware;
using WCCG.PAS.Referrals.API.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.API.Integration.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();

    [Fact]
    public async Task ShouldHandleDeserializationFailedException()
    {
        //Arrange
        var exception = _fixture.Create<DeserializationFailedException>();

        var host = StartHost(exception);

        //Act
        var response = await host.GetTestClient().GetAsync(HostProvider.TestEndpoint);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        problemDetails.Detail.Should().Be(exception.Message);
    }

    [Fact]
    public async Task ShouldHandleJsonException()
    {
        //Arrange
        var exception = _fixture.Create<JsonException>();
        var host = StartHost(exception);

        //Act
        var response = await host.GetTestClient().GetAsync(HostProvider.TestEndpoint);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        problemDetails.Detail.Should().Be(exception.Message);
    }

    [Fact]
    public async Task ShouldHandleValidationException()
    {
        //Arrange
        var exception = new ValidationException(_fixture.CreateMany<ValidationFailure>());
        var host = StartHost(exception);

        var expectedExtensions = JsonSerializer.Serialize(exception.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        //Act
        var response = await host.GetTestClient().GetAsync(HostProvider.TestEndpoint);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        problemDetails.Extensions["validationErrors"]?.ToString().Should().BeEquivalentTo(expectedExtensions);
    }

    [Fact]
    public async Task ShouldHandleCosmosException()
    {
        //Arrange
        var exception = _fixture.Create<CosmosException>();
        var host = StartHost(exception);

        //Act
        var response = await host.GetTestClient().GetAsync(HostProvider.TestEndpoint);

        //Assert
        response.StatusCode.Should().Be(exception.StatusCode);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        problemDetails.Detail.Should().Be(exception.Message);
    }

    [Fact]
    public async Task ShouldHandleDefaultException()
    {
        //Arrange
        var exception = _fixture.Create<Exception>();
        var host = StartHost(exception);

        //Act
        var response = await host.GetTestClient().GetAsync(HostProvider.TestEndpoint);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        problemDetails.Detail.Should().Be(exception.Message);
    }

    private static IHost StartHost(Exception exception)
    {
        return HostProvider.StartHostWithEndpoint(_ => throw exception,
            configureApp: app => app.UseMiddleware<ExceptionHandlingMiddleware>());
    }
}
