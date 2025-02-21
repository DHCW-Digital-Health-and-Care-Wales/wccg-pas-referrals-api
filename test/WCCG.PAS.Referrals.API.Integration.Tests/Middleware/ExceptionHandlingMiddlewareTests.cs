using System.Net;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Builder;
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
        (await response.Content.ReadAsStringAsync()).Should().Be(exception.Message);
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
        (await response.Content.ReadAsStringAsync()).Should().Be(exception.Message);
    }

    [Fact]
    public async Task ShouldHandleValidationException()
    {
        //Arrange
        var exception = _fixture.Create<ValidationException>();
        var host = StartHost(exception);

        var expectedBody = JsonSerializer.Serialize(exception.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        //Act
        var response = await host.GetTestClient().GetAsync(HostProvider.TestEndpoint);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        (await response.Content.ReadAsStringAsync()).Should().Be(expectedBody);
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
        (await response.Content.ReadAsStringAsync()).Should().Be(exception.Message);
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
    }

    private static IHost StartHost(Exception exception)
    {
        return HostProvider.StartHostWithEndpoint(_ => throw exception,
            configureApp: app => app.UseMiddleware<ExceptionHandlingMiddleware>());
    }
}
