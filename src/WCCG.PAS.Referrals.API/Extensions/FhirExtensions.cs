using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Extensions;

public static class FhirExtensions
{
    public static T2? SelectNestedWithCondition<T1, T2>(this IEnumerable<T1> resourceList,
        Func<T1?, IEnumerable<T2>?> nestedList,
        Func<T2?, string?> conditionProp,
        string conditionValue) where T1 : Base where T2 : Base
    {
        foreach (var resource in resourceList)
        {
            var innerList = nestedList(resource);
            if (innerList is null)
            {
                continue;
            }

            foreach (var nestedResource in innerList)
            {
                var propertyToCheck = conditionProp(nestedResource);
                if (propertyToCheck is not null && propertyToCheck.Equals(conditionValue, StringComparison.OrdinalIgnoreCase))
                {
                    return nestedResource;
                }
            }
        }

        return null;
    }

    public static T? SelectWithConditions<T>(this IEnumerable<T> resourceList,
        (Func<T, string?> conditionProp, string conditionValue)[] conditions)
        where T : Base
    {
        return resourceList.FirstOrDefault(x =>
        {
            return conditions.All(condition =>
            {
                var propertyToCheck = condition.conditionProp(x);
                return propertyToCheck is not null && propertyToCheck.Equals(condition.conditionValue, StringComparison.OrdinalIgnoreCase);
            });
        });
    }

    public static T? SelectWithCondition<T>(this IEnumerable<T> resourceList, Func<T, string?> conditionProp, string conditionValue)
        where T : Base
    {
        return resourceList.FirstOrDefault(x =>
        {
            var propertyToCheck = conditionProp(x);
            return propertyToCheck is not null && propertyToCheck.Equals(conditionValue, StringComparison.OrdinalIgnoreCase);
        });
    }

    public static string? GetStringValueByElementName(this IEnumerable<ElementValue> values, string elementName)
    {
        var element = values.FirstOrDefault(x => x.ElementName.Equals(elementName, StringComparison.OrdinalIgnoreCase));

        return element.Value.TypeName.Equals(nameof(String), StringComparison.OrdinalIgnoreCase)
            ? element.Value.ToString()
            : null;
    }

    public static TOut? GetResourceByType<TOut>(this Bundle bundle) where TOut : Resource
    {
        var res = bundle.Children.OfType<Bundle.EntryComponent>()
            .FirstOrDefault(x => x.Resource is not null
                                 && x.Resource.TypeName.Equals(typeof(TOut).Name, StringComparison.OrdinalIgnoreCase));

        return res?.Resource as TOut;
    }

    public static T? GetResourceByUrl<T>(this Bundle bundle, string? url) where T : Resource
    {
        var entryComponent = bundle.Children.OfType<Bundle.EntryComponent>()
            .FirstOrDefault(x => x.FullUrl is not null
                                 && x.FullUrl.Equals(url, StringComparison.OrdinalIgnoreCase));

        return entryComponent?.Resource as T;
    }

    public static T? GetResourceByUrlWithCondition<T>(this Bundle bundle,
        string? url,
        Func<T, string?> conditionProp,
        string conditionValue) where T : Resource
    {
        var entryComponent = bundle.Children.OfType<Bundle.EntryComponent>()
            .FirstOrDefault(x => x.FullUrl is not null
                                 && x.FullUrl.Equals(url, StringComparison.OrdinalIgnoreCase));

        if (entryComponent?.Resource is not T tResource)
        {
            return null;
        }

        var propertyToCheck = conditionProp(tResource);
        return propertyToCheck is null
            ? null
            : propertyToCheck.Equals(conditionValue, StringComparison.OrdinalIgnoreCase)
                ? tResource
                : null;
    }

    public static T? GetResourceByIdFromList<T>(this IEnumerable<ResourceReference> references, Bundle? bundle, string id)
        where T : Resource
    {
        return references.Select(reference => bundle?.GetResourceByUrl<T>(reference.Reference))
            .FirstOrDefault(resource => resource?.Id is not null
                                        && resource.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public static void EnrichForResponse(this Bundle bundle, ReferralDbModel dbModel)
    {
        var serviceRequest = bundle.GetResourceByType<ServiceRequest>()!;
        var patient = bundle.GetResourceByUrl<Patient>(serviceRequest.Subject.Reference)!;
        var encounter = bundle.GetResourceByUrl<Encounter>(serviceRequest.Encounter.Reference)!;
        var appointment = bundle.GetResourceByUrl<Appointment>(encounter.Appointment.FirstOrDefault()!.Reference)!;

        patient.Identifier
            .SelectWithCondition(x => x.System, NhsFhirConstants.PasIdentifierSystem)
            !.Value = dbModel.CaseNumber;

        appointment.Created = dbModel.BookingDate;

        serviceRequest.Identifier.SelectWithCondition(x => x.System, NhsFhirConstants.ReferralIdSystem)
            !.Value = dbModel.ReferralId;
    }
}
