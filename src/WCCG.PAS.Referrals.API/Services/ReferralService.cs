using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Extensions;
using WCCG.PAS.Referrals.API.Helpers;
using WCCG.PAS.Referrals.API.Mappers;
using WCCG.PAS.Referrals.API.Repositories;

namespace WCCG.PAS.Referrals.API.Services;

public class ReferralService : IReferralService
{
    private readonly IReferralMapper _mapper;
    private readonly IReferralCosmosRepository _repository;
    private readonly IBundleCreator _bundleCreator;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IValidator<ReferralDbModel> _validator;

    public ReferralService(
        IReferralMapper mapper,
        IReferralCosmosRepository repository,
        IBundleCreator bundleCreator,
        JsonSerializerOptions jsonSerializerOptions,
        IValidator<ReferralDbModel> validator)
    {
        _mapper = mapper;
        _repository = repository;
        _bundleCreator = bundleCreator;
        _jsonSerializerOptions = jsonSerializerOptions;
        _validator = validator;
    }

    public async Task<string> CreateReferralAsync(string bundleJson)
    {
        var bundle = JsonSerializer.Deserialize<Bundle>(bundleJson, _jsonSerializerOptions)!;
        var referralDbModel = _mapper.MapFromBundle(bundle);

        var validationResult = await _validator.ValidateAsync(referralDbModel);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await _repository.CreateReferralAsync(referralDbModel);

        bundle.EnrichForResponse(referralDbModel);
        return JsonSerializer.Serialize(bundle, _jsonSerializerOptions);
    }

    public async Task<string> GetReferralAsync(string referralId)
    {
        if (!Guid.TryParse(referralId, out _))
        {
            throw new ValidationException([new ValidationFailure(nameof(referralId), "Referral Id should be a valid GUID.")]);
        }

        var referralDbModel = await _repository.GetReferralAsync(referralId);

        var bundle = _bundleCreator.CreateBundle(referralDbModel);
        return JsonSerializer.Serialize(bundle, _jsonSerializerOptions);
    }
}
