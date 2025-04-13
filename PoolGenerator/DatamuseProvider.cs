using DatamuseDotNet;

namespace PoolGenerator;

internal sealed class DatamuseProvider
{
    private DatamuseProvider(DatamuseClient client, List<string> initialPool,
        MaxResultsModifier maxResultsModifier)
    {
        _client = client;
        _initialPool = initialPool;
        _maxResultsModifier = maxResultsModifier;
    }

    public IEnumerable<string> GetDistinctStrings() => GetAllStrings().Distinct();

    private IEnumerable<string> GetAllStrings()
    {
        foreach (string initial in _initialPool)
        {
            yield return initial;

            List<string>? derived = GetStrings(_client, initial, _maxResultsModifier);
            if (derived is null)
            {
                continue;
            }
            foreach (string d in derived)
            {
                yield return d;
            }
        }
    }

    public static DatamuseProvider? TryCreate(string wordsMeaning, ushort maxRequestSize)
    {
        DatamuseClient client = new();
        MaxResultsModifier maxResultsModifier = new(maxRequestSize);

        List<string>? wordsPool = GetStrings(client, wordsMeaning, maxResultsModifier);

        return wordsPool is null ? null : new DatamuseProvider(client, wordsPool, maxResultsModifier);
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private static List<string>? GetStrings(DatamuseClient client, string wordsMeaning,
        MaxResultsModifier maxResultsModifier)
    {
        MeansLikeModifier meansLikeModifier = new(wordsMeaning);
        return client.Words(meansLikeModifier, maxResultsModifier)
                     ?.Select(x => x.word)
                     .ToList();
    }

    private readonly DatamuseClient _client;
    private readonly List<string> _initialPool;
    private readonly MaxResultsModifier _maxResultsModifier;
}