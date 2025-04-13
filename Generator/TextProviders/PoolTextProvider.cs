namespace Generator.TextProviders;

internal sealed class PoolTextProvider : ITextProvider
{
    public IEnumerable<int> GetLengths() => _textPool.Keys;

    public PoolTextProvider(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        _textPool = lines.Where(l => !string.IsNullOrWhiteSpace(l))
                         .GroupBy(l => l.Length)
                         .ToDictionary(g => g.Key, g => (IReadOnlyList<string>) g.ToList());
    }

    public string GetText(int length)
    {
        return _textPool.ContainsKey(length)
            ? _textPool[length].GetRandomElement()
            : throw new ArgumentOutOfRangeException($"Can't retreive string of length {length}");
    }

    private readonly IReadOnlyDictionary<int, IReadOnlyList<string>> _textPool;
}