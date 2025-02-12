using AutoFixture;
using FluentAssertions;
using Moq;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Pages;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Pages;

public class IndexModelTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();
    private readonly IndexModel _sut;

    public IndexModelTests()
    {
        _sut = new IndexModel(_fixture.Mock<IReferralService>().Object);
    }

    [Fact]
    public async Task OnGetShouldCallGetAllAsync()
    {
        //Act
        await _sut.OnGet();

        //Assert
        _fixture.Mock<IReferralService>().Verify(r => r.GetAllAsync());
    }

    [Fact]
    public async Task OnGetShouldSetReferrals()
    {
        //Arrange
        var allReferrals = _fixture.CreateMany<ReferralDbModel>().ToList();
        _fixture.Mock<IReferralService>().Setup(r => r.GetAllAsync())
            .ReturnsAsync(allReferrals);

        //Act
        await _sut.OnGet();

        //Assert
        _sut.Referrals.Should().BeEquivalentTo(allReferrals);
    }
}
