using System.Text;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WCCG.PAS.Referrals.API.Controllers;
using WCCG.PAS.Referrals.API.Services;
using WCCG.PAS.Referrals.API.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.API.Unit.Tests.Controllers;

public class ReferralsControllerTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();

    private readonly ReferralsController _sut;

    public ReferralsControllerTests()
    {
        _fixture.OmitAutoProperties = true;
        _sut = _fixture.CreateWithFrozen<ReferralsController>();
    }

    [Fact]
    public async Task CreateReferralShouldCallCreateReferralAsync()
    {
        //Arrange
        var bodyValue = _fixture.Create<string>();
        SetRequestBody(bodyValue);

        //Act
        await _sut.CreateReferral();

        //Assert
        _fixture.Mock<IReferralService>().Verify(x => x.CreateReferralAsync(bodyValue));
    }

    [Fact]
    public async Task CreateReferralShouldReturn200WhenCreated()
    {
        //Arrange

        var bodyValue = _fixture.Create<string>();
        SetRequestBody(bodyValue);

        var outputBundleJson = _fixture.Create<string>();

        _fixture.Mock<IReferralService>().Setup(x => x.CreateReferralAsync(It.IsAny<string>()))
            .ReturnsAsync(outputBundleJson);

        //Act
        var result = await _sut.CreateReferral();

        //Assert
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.Content.Should().Be(outputBundleJson);
        contentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        contentResult.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task CreateReferralShouldReturn400WhenNotCreated()
    {
        //Arrange
        var bodyValue = _fixture.Create<string>();
        SetRequestBody(bodyValue);

        _fixture.Mock<IReferralService>().Setup(x => x.CreateReferralAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        //Act
        var result = await _sut.CreateReferral();

        //Assert
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.Content.Should().Be(bodyValue);
        contentResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        contentResult.ContentType.Should().Be("application/json");
    }

    private void SetRequestBody(string value)
    {
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _sut.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(value));
        _sut.ControllerContext.HttpContext.Request.ContentLength = value.Length;
    }
}
