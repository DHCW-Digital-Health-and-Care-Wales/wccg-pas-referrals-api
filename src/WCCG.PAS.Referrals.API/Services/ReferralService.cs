using FluentValidation;
using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Helpers;
using WCCG.PAS.Referrals.API.Mappers;
using WCCG.PAS.Referrals.API.Repositories;

namespace WCCG.PAS.Referrals.API.Services;

public class ReferralService : IReferralService
{
    private readonly IReferralMapper _mapper;
    private readonly IReferralCosmosRepository _repository;
    private readonly IBundleFiller _bundleFiller;
    private readonly IValidator<ReferralDbModel> _validator;

    public ReferralService(
        IReferralMapper mapper,
        IReferralCosmosRepository repository,
        IBundleFiller bundleFiller,
        IValidator<ReferralDbModel> validator)
    {
        _mapper = mapper;
        _repository = repository;
        _bundleFiller = bundleFiller;
        _validator = validator;
    }

    public async Task<Bundle> CreateReferralAsync(Bundle bundle)
    {
        var referralDbModel = _mapper.MapFromBundle(bundle);

        var validationResult = await _validator.ValidateAsync(referralDbModel);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        _bundleFiller.AdjustBundleWithDbModelData(bundle, referralDbModel);
        await _repository.CreateReferralAsync(referralDbModel);

        return bundle;
    }
}
