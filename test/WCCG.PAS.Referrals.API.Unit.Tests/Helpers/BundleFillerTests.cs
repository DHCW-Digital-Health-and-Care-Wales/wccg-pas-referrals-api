using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Helpers;
using WCCG.PAS.Referrals.API.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.API.Unit.Tests.Helpers;

public class BundleFillerTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();

    private readonly BundleFiller _sut;

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        .ForFhir(ModelInfo.ModelInspector)
        .UsingMode(DeserializerModes.BackwardsCompatible);

    public BundleFillerTests()
    {
        _sut = _fixture.CreateWithFrozen<BundleFiller>();
    }

    [Fact]
    public void AdjustBundleWithDbModelDataShouldSetNewValues()
    {
        //Arrange
        var originalBundleJson = File.ReadAllText(@"TestData\example-bundle.json");
        var originalBundle = JsonSerializer.Deserialize<Bundle>(originalBundleJson, _options);

        var expectedBundleJson = File.ReadAllText(@"TestData\example-bundle-adjusted.json");
        var expectedBundle = JsonSerializer.Deserialize<Bundle>(expectedBundleJson, _options);

        var dbModel = _fixture.Build<ReferralDbModel>()
            .With(x => x.ReferralId, "b5e07b94-a9f3-4be0-8f05-65cfc099732c")
            .With(x => x.CaseNumber, "7dc1da78-021c-4423-acb8-01751bc72a25")
            .With(x => x.BookingDate, "2025-02-19T13:25:19.2918349Z")
            .Create();

        //Act
        _sut.AdjustBundleWithDbModelData(originalBundle!, dbModel);

        //Assert
        originalBundle.Should().BeEquivalentTo(expectedBundle);
    }
}
