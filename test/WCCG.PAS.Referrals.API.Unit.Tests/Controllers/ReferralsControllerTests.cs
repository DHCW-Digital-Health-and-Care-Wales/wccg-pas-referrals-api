using System.Text;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.Controllers.v1;
using WCCG.PAS.Referrals.API.Services;
using WCCG.PAS.Referrals.API.Unit.Tests.Extensions;
using Task = System.Threading.Tasks.Task;

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
        var bundleJson = _fixture.Create<string>();
        SetRequestBody(bundleJson);

        //Act
        await _sut.CreateReferral();

        //Assert
        _fixture.Mock<IReferralService>().Verify(x => x.CreateReferralAsync(bundleJson));
    }

    [Fact]
    public async Task CreateReferralShouldReturn200()
    {
        //Arrange
        SetRequestBody(_fixture.Create<string>());

        var outputBundleJson = _fixture.Create<string>();

        _fixture.Mock<IReferralService>().Setup(x => x.CreateReferralAsync(It.IsAny<string>()))
            .ReturnsAsync(outputBundleJson);

        //Act
        var result = await _sut.CreateReferral();

        //Assert
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.StatusCode.Should().Be(200);
        contentResult.ContentType.Should().Be(FhirConstants.FhirMediaType);
        contentResult.Content.Should().Be(outputBundleJson);
    }

    [Fact]
    public async Task GetReferralShouldCallGetReferralAsync()
    {
        //Arrange
        var referralId = _fixture.Create<string>();

        //Act
        await _sut.GetReferral(referralId);

        //Assert
        _fixture.Mock<IReferralService>().Verify(x => x.GetReferralAsync(referralId));
    }

    [Fact]
    public async Task GetReferralShouldReturn200()
    {
        //Arrange
        var referralId = _fixture.Create<string>();
        var outputBundleJson = _fixture.Create<string>();

        _fixture.Mock<IReferralService>().Setup(x => x.GetReferralAsync(It.IsAny<string>()))
            .ReturnsAsync(outputBundleJson);

        //Act
        var result = await _sut.GetReferral(referralId);

        //Assert
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.StatusCode.Should().Be(200);
        contentResult.ContentType.Should().Be(FhirConstants.FhirMediaType);
        contentResult.Content.Should().Be(outputBundleJson);
    }

    private void SetRequestBody(string value)
    {
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _sut.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(value));
        _sut.ControllerContext.HttpContext.Request.ContentLength = value.Length;
    }
}
