using AutoFixture;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WCCG.PAS.Referrals.API.Controllers;
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
        var bundle = _fixture.Create<Bundle>();

        //Act
        await _sut.CreateReferral(bundle);

        //Assert
        _fixture.Mock<IReferralService>().Verify(x => x.CreateReferralAsync(bundle));
    }

    [Fact]
    public async Task CreateReferralShouldReturn200()
    {
        //Arrange
        var bundle = _fixture.Create<Bundle>();
        var outputBundle = _fixture.Create<Bundle>();

        _fixture.Mock<IReferralService>().Setup(x => x.CreateReferralAsync(It.IsAny<Bundle>()))
            .ReturnsAsync(outputBundle);

        //Act
        var result = await _sut.CreateReferral(bundle);

        //Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(outputBundle);
    }
}
