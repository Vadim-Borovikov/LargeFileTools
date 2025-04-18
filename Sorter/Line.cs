using Sorter.Config;

namespace Sorter;

internal sealed record Line(int Number, string Text) : IComparable<Line>
{
    static Line() => Separator = string.Format(LineFormat, string.Empty, string.Empty);

    public int CompareTo(Line? other)
    {
        if (other is null)
        {
            return 1;
        }

        int textCompare = string.Compare(Text, other.Text, StringComparison.Ordinal);
        return textCompare != 0 ? textCompare : Number.CompareTo(other.Number);
    }

    public override string ToString() => string.Format(LineFormat, Number, Text);

    public static Line Parse(string source)
    {
        return TryParse(source) ?? throw new FormatException($"Invalid line format: {source}");
    }

    public static Line? TryParse(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        ReadOnlySpan<char> span = source;

        int separatorIndex = span.IndexOf(Separator, StringComparison.Ordinal);
        if (separatorIndex <= 0)
        {
            return null;
        }

        if (!int.TryParse(span.Slice(0, separatorIndex), out int number))
        {
            return null;
        }

        int textStart = separatorIndex + Separator.Length;
        if (textStart >= span.Length)
        {
            return null;
        }

        string text = span.Slice(textStart).ToString();

        return new Line(number, text);
    }

    private static readonly string LineFormat = Provider.Config.LineFormat;
    private static readonly string Separator;
}