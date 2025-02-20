using Hl7.Fhir.Model;

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

    public static T? SelectWithCondition<T>(this IEnumerable<T> resourceList, Func<T, string?> conditionProp, string conditionValue)
        where T : Base
    {
        return resourceList.FirstOrDefault(x =>
        {
            var propertyToCheck = conditionProp(x);
            return propertyToCheck is not null && propertyToCheck.Equals(conditionValue, StringComparison.OrdinalIgnoreCase);
        });
    }

    public static T? CheckPropertyValue<T>(this T? resource, Func<T?, string?> conditionProp, string conditionValue) where T : Base
    {
        var propertyToCheck = conditionProp(resource);

        return propertyToCheck is null
            ? null
            : propertyToCheck.Equals(conditionValue, StringComparison.OrdinalIgnoreCase)
                ? resource
                : null;
    }

    public static string? GetStringValueByElementName(this IEnumerable<ElementValue> values, string elementName)
    {
        var element = values.FirstOrDefault(x => x.ElementName.Equals(elementName, StringComparison.OrdinalIgnoreCase));

        return element.Value.TypeName.Equals(nameof(String), StringComparison.OrdinalIgnoreCase)
            ? element.Value.ToString()
            : null;
    }

    public static TOut? GetResourceByType<TOut>(this Bundle bundle) where TOut : Base
    {
        var res = bundle.Children.OfType<Bundle.EntryComponent>()
            .FirstOrDefault(x => x.Resource is not null
                                 && x.Resource.TypeName.Equals(typeof(TOut).Name, StringComparison.OrdinalIgnoreCase));

        return res?.Resource as TOut;
    }

    public static Resource? GetResourceByUrl(this Bundle bundle, string? url)
    {
        var entryComponent = bundle.Children.OfType<Bundle.EntryComponent>()
            .FirstOrDefault(x => x.FullUrl is not null
                                 && x.FullUrl.Equals(url, StringComparison.OrdinalIgnoreCase));

        return entryComponent?.Resource;
    }

    public static Resource? GetResourceByIdFromList(this IEnumerable<ResourceReference> references, Bundle? bundle, string id)
    {
        return references.Select(reference => bundle?.GetResourceByUrl(reference.Reference))
            .FirstOrDefault(resource => resource?.Id is not null
                                        && resource.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}
