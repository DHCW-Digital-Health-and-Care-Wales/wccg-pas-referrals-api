using Microsoft.Extensions.Options;

namespace WCCG.PAS.Referrals.API.Configuration.OptionValidators;

[OptionsValidator]
public partial class ValidateBundleCreationConfigOptions : IValidateOptions<BundleCreationConfig>;
