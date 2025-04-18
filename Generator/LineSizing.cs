using Generator.TextProviders;

namespace Generator;

internal readonly struct LineSizing
{
    public const byte MinNumberLength = 1;

    public static readonly int MaxNumberLength = int.MaxValue.ToString().Length;

    public readonly byte MinTextLength;
    public readonly int MaxTextLength;
    public readonly int LineDecorationLength;
    public readonly int MinLineLength;
    public readonly int MaxLineLength;

    public LineSizing(ITextProvider textProvider, string lineFormat)
    {
        MinTextLength = ITextProvider.MinLength;
        MaxTextLength = textProvider.MaxLength;

        LineDecorationLength =
            string.Format(lineFormat, string.Empty, string.Empty).Length + Environment.NewLine.Length;

        MinLineLength = MinNumberLength + MinTextLength + LineDecorationLength;
        MaxLineLength = MaxNumberLength + MaxTextLength + LineDecorationLength;
    }
}