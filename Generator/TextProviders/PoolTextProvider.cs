namespace Generator.TextProviders;

internal sealed class PoolTextProvider : ITextProvider
{
    public int MaxLength => _textPool.Count;
    public int TextsAmount => _textPool.Sum(l => l.Count);

    private PoolTextProvider(IReadOnlyList<IReadOnlyList<string>> textPool) => _textPool = textPool;

    public static PoolTextProvider? TryCreateFrom(string filePath, out string? error)
    {
        error = null;
        string[] texts = File.ReadAllLines(filePath);

        Dictionary<int, List<string>> buckets = texts.Where(t => !string.IsNullOrWhiteSpace(t)
                                                                 && (t.Length >= ITextProvider.MinLength))
                                                     .GroupBy(t => t.Length)
                                                     .ToDictionary(g => g.Key, g => g.ToList());
        if (!buckets.ContainsKey(ITextProvider.MinLength))
        {
            error = $"Dictionary lacks words with length of {ITextProvider.MinLength}";
            return null;
        }

        int maxLength = buckets.Keys.Max();
        for (int length = ITextProvider.MinLength + 1; length < maxLength; ++length)
        {
            if (!buckets.ContainsKey(length))
            {
                maxLength = length - 1;
                break;
            }
        }

        List<IReadOnlyList<string>> pool = new(maxLength);
        for (int i = 1; i <= maxLength; i++)
        {
            pool.Add(buckets[i]);
        }

        return new PoolTextProvider(pool);
    }

    public string GetText()
    {
        int length = Random.Shared.Next(ITextProvider.MinLength, MaxLength + 1);
        return GetText(length);
    }

    public string GetText(int length)
    {
        return (length >= ITextProvider.MinLength) && (length <= MaxLength)
            ? Random.Shared.PickFrom(_textPool[length - 1])
            : throw new ArgumentOutOfRangeException($"Text length must be between {ITextProvider.MinLength} and {MaxLength}");
    }

    private readonly IReadOnlyList<IReadOnlyList<string>> _textPool;
}