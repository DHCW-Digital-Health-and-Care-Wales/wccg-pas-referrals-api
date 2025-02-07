using AutoFixture;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

public class OmitRecursionCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
