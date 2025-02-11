using FluentValidation;
using WCCG.PAS.Referrals.UI.Models;

namespace WCCG.PAS.Referrals.UI.Validators;

public class ReferralValidator : AbstractValidator<Referral>
{
    public ReferralValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CaseNumber).NotEmpty();
    }
}
