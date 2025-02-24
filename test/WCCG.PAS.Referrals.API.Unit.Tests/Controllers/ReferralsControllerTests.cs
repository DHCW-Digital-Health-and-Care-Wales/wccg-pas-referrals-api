using System.Text;
using AutoFixture;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
    public async Task CreateReferralShouldDeserializeBody()
    {
        //Arrange
        var bundle = _fixture.Create<string>();
        SetRequestBody(bundle);

        //Act
        await _sut.CreateReferral();

        //Assert
        _fixture.Mock<IFhirBundleSerializer>().Verify(x => x.Deserialize(bundle));
    }

    [Fact]
    public async Task CreateReferralShouldCallCreateReferralAsync()
    {
        //Arrange
        SetRequestBody(_fixture.Create<string>());

        var bundle = _fixture.Create<Bundle>();
        _fixture.Mock<IFhirBundleSerializer>().Setup(x => x.Deserialize(It.IsAny<string>()))
            .Returns(bundle);

        //Act
        await _sut.CreateReferral();

        //Assert
        _fixture.Mock<IReferralService>().Verify(x => x.CreateReferralAsync(bundle));
    }

    [Fact]
    public async Task CreateReferralShouldSerializeOutputBundle()
    {
        //Arrange
        SetRequestBody(_fixture.Create<string>());

        var outputBundle = _fixture.Create<Bundle>();

        _fixture.Mock<IReferralService>().Setup(x => x.CreateReferralAsync(It.IsAny<Bundle>()))
            .ReturnsAsync(outputBundle);

        //Act
        await _sut.CreateReferral();

        //Assert
        _fixture.Mock<IFhirBundleSerializer>().Verify(x => x.Serialize(outputBundle));
    }

    [Fact]
    public async Task CreateReferralShouldReturn200()
    {
        //Arrange
        SetRequestBody(_fixture.Create<string>());

        var outputBundleJson = _fixture.Create<string>();

        _fixture.Mock<IFhirBundleSerializer>().Setup(x => x.Serialize(It.IsAny<Bundle>()))
            .Returns(outputBundleJson);

        //Act
        var result = await _sut.CreateReferral();

        //Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(outputBundleJson);
    }

    private void SetRequestBody(string value)
    {
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _sut.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(value));
        _sut.ControllerContext.HttpContext.Request.ContentLength = value.Length;
    }
}
