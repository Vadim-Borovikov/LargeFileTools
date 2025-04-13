namespace Generator;

internal readonly struct LineSizing
{
    public const byte MinNumberLength = 1;

    public static readonly int MaxNumberLength = int.MaxValue.ToString().Length;

    public static readonly int LineBreakLength = Environment.NewLine.Length;

    public readonly int MinTextLength;
    public readonly int LineDecorationLength;
    public readonly int MinLineLength;
    public readonly int MaxLineLength;
    public readonly int DefaultLineContextLength;
    public readonly int DefaultAdditionSize;

    private LineSizing(int minTextLength, int maxTextLength, int lineDecorationLength)
    {
        MinTextLength = minTextLength;
        LineDecorationLength = lineDecorationLength;

        MinLineLength = MinNumberLength + MinTextLength + LineDecorationLength;
        MaxLineLength = MaxNumberLength + maxTextLength + LineDecorationLength;

        DefaultLineContextLength = MinNumberLength + maxTextLength;
        DefaultAdditionSize = DefaultLineContextLength + LineDecorationLength + LineBreakLength;
    }

    public static LineSizing? TryCreate(HashSet<int> lengths, string lineFormat, out string? error)
    {
        int minTextLength = lengths.Min();
        int maxTextLength = MaxNumberLength + minTextLength - MinNumberLength;

        for (int l = minTextLength + 1; l <= maxTextLength; ++l)
        {
            if (!lengths.Contains(l))
            {
                error = $"Dictionary lacks words with length of {l}";
                return null;
            }
        }

        int lineDecorationLength = string.Format(lineFormat, "", "").Length;

        error = null;
        return new LineSizing(minTextLength, maxTextLength, lineDecorationLength);
    }
}