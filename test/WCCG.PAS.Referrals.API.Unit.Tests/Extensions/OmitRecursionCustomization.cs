using AutoFixture;

namespace WCCG.PAS.Referrals.API.Unit.Tests.Extensions;

public class OmitRecursionCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
