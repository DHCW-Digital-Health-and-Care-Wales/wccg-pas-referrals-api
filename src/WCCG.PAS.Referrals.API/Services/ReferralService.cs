using FluentValidation;
using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Extensions;
using WCCG.PAS.Referrals.API.Mappers;
using WCCG.PAS.Referrals.API.Repositories;

namespace WCCG.PAS.Referrals.API.Services;

public class ReferralService : IReferralService
{
    private readonly IReferralMapper _mapper;
    private readonly IReferralCosmosRepository _repository;
    private readonly IValidator<ReferralDbModel> _validator;

    public ReferralService(
        IReferralMapper mapper,
        IReferralCosmosRepository repository,
        IValidator<ReferralDbModel> validator)
    {
        _mapper = mapper;
        _repository = repository;
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

        await _repository.CreateReferralAsync(referralDbModel);

        bundle.EnrichForResponse(referralDbModel);
        return bundle;
    }
}
