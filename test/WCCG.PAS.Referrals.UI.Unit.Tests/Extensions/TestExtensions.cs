using FluentAssertions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

public static class TestExtensions
{
    public static bool IsEquivalentTo(this object first, object second)
    {
        try
        {
            first.Should().BeEquivalentTo(second);
        }
        catch
        {
            return false;
        }

        return true;
    }
}
