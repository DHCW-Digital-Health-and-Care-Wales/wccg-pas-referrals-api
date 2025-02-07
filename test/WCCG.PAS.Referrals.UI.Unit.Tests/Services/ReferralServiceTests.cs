using AutoFixture;
using FluentAssertions;
using Moq;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Repositories;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Services
{
    public class ReferralServiceTests
    {
        private readonly IFixture _fixture = new Fixture().WithCustomizations();
        private readonly ReferralService _sut;

        public ReferralServiceTests()
        {
            _sut = _fixture.CreateWithFrozen<ReferralService>();
        }

        [Fact]
        public async Task UpsertAsync_Should_CallRepoMethod()
        {
            //Arrange
            var referral = _fixture.Create<Referral>();
            var upsertResult = _fixture.Create<bool>();

            _fixture.Mock<ICosmosRepository<Referral>>().Setup(r => r.UpsertAsync(It.IsAny<Referral>()))
                .ReturnsAsync(upsertResult);

            //Act
            var result = await _sut.UpsertAsync(referral);

            //Assert
            result.Should().Be(upsertResult);
            _fixture.Mock<ICosmosRepository<Referral>>().Verify(r => r.UpsertAsync(referral));
        }

        [Fact]
        public async Task GetAllAsync_Should_CallRepoMethod()
        {
            //Arrange
            var allReferrals = _fixture.CreateMany<Referral>().ToList();

            _fixture.Mock<ICosmosRepository<Referral>>().Setup(r => r.GetAllAsync())
                .ReturnsAsync(allReferrals);

            //Act
            var result = await _sut.GetAllAsync();

            //Assert
            result.Should().BeEquivalentTo(allReferrals);
            _fixture.Mock<ICosmosRepository<Referral>>().Verify(r => r.GetAllAsync());
        }

        [Fact]
        public async Task GetByIdAsync_Should_CallRepoMethod()
        {
            //Arrange
            var id = _fixture.Create<string>();
            var referral = _fixture.Create<Referral>();

            _fixture.Mock<ICosmosRepository<Referral>>().Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(referral);

            //Act
            var result = await _sut.GetByIdAsync(id);

            //Assert
            result.Should().BeEquivalentTo(referral);
            _fixture.Mock<ICosmosRepository<Referral>>().Verify(r => r.GetByIdAsync(id));
        }
    }
}
