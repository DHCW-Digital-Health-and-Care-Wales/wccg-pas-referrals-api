using FluentValidation;
using WCCG.PAS.Referrals.UI.DbModels;

namespace WCCG.PAS.Referrals.UI.Validators;

public class ReferralValidator : AbstractValidator<ReferralDbModel>
{
    public ReferralValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CaseNumber).NotEmpty();
    }
}
