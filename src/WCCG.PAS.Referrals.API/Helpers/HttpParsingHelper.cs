using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.API.Helpers;

// https://github.com/microsoft/ApplicationInsights-dotnet/blob/main/WEB/Src/DependencyCollector/DependencyCollector/Implementation/HttpParsers/HttpParsingHelper.cs
[ExcludeFromCodeCoverage]
public static class HttpParsingHelper
{
    private static readonly char[] RequestPathEndDelimiters = ['?', '#'];

    private static readonly char[] RequestPathTokenDelimiters = ['/'];

    /// <summary>
    /// Builds a resource operation moniker in the format of "VERB /a/*/b/*/c".
    /// </summary>
    /// <param name="verb">The HTTP verb.</param>
    /// <param name="resourcePath">The resource path represented as a list of resource type and resource ID pairs.</param>
    /// <returns>Operation moniker string.</returns>
    internal static string BuildOperationMoniker(string verb, List<KeyValuePair<string, string>> resourcePath)
    {
        var tokens = new List<string>((4 * resourcePath.Count) + 2);

        if (!string.IsNullOrEmpty(verb))
        {
            tokens.Add(verb);
            tokens.Add(" ");
        }

        foreach (var resource in resourcePath)
        {
            tokens.Add("/");
            tokens.Add(resource.Key);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (resource.Value != null)
            {
                tokens.Add("/*");
            }
        }

        return string.Concat(tokens);
    }

    /// <summary>
    /// Parses request path into REST resource path represented as a list of resource type and resource ID pairs.
    /// </summary>
    /// <param name="requestPath">The request path.</param>
    /// <returns>A list of resource type and resource ID pairs.</returns>
    internal static List<KeyValuePair<string, string>> ParseResourcePath(string requestPath)
    {
        var tokens = TokenizeRequestPath(requestPath);

        var pairCount = (tokens.Count + 1) / 2;
        var results = new List<KeyValuePair<string, string>>(pairCount);
        for (var i = 0; i < pairCount; i++)
        {
            var keyIdx = 2 * i;
            var valIdx = keyIdx + 1;
            var key = tokens[keyIdx];
            var value = valIdx == tokens.Count ? null : tokens[valIdx];
            if (!string.IsNullOrEmpty(key))
            {
                results.Add(new KeyValuePair<string, string>(key, value!));
            }
        }

        return results;
    }

    /// <summary>
    /// Tokenizes request path.
    /// E.g. the string "/a/b/c/d?e=f" will be tokenized into [ "a", "b", "c", "d" ].
    /// </summary>
    /// <param name="requestPath">The request path.</param>
    /// <returns>List of tokens.</returns>
    private static List<string> TokenizeRequestPath(string requestPath)
    {
        var slashPrefixShift = requestPath[0] == '/' ? 1 : 0;
        var endIdx = requestPath.IndexOfAny(RequestPathEndDelimiters, slashPrefixShift);
        var tokens = Split(requestPath, RequestPathTokenDelimiters, slashPrefixShift, endIdx);

        return tokens;
    }

    /// <summary>
    /// Splits substring by given delimiters.
    /// </summary>
    /// <param name="str">The string to split.</param>
    /// <param name="delimiters">The delimiters.</param>
    /// <param name="startIdx">
    /// The index at which splitting will start.
    /// This is not validated and expected to be within input string range.
    /// </param>
    /// <param name="endIdx">
    /// The index at which splitting will end.
    /// If -1 then string will be split till it's end.
    /// This is not validated and expected to be less than string length.
    /// </param>
    /// <returns>A list of substrings.</returns>
    private static List<string> Split(string str, char[] delimiters, int startIdx, int endIdx)
    {
        if (endIdx < 0)
        {
            endIdx = str.Length;
        }

        if (endIdx <= startIdx)
        {
            return new List<string>(0);
        }

        var results = new List<string>(16);

        var idx = startIdx;
        while (idx <= endIdx)
        {
            var cutIdx = str.IndexOfAny(delimiters, idx, endIdx - idx);
            if (cutIdx < 0)
            {
                cutIdx = endIdx;
            }

            results.Add(str.Substring(idx, cutIdx - idx));
            idx = cutIdx + 1;
        }

        return results;
    }
}
