namespace PoolGenerator;

internal static class StringExtensions
{
    public static string Capitalize(this string source)
    {
        return source.Length switch
        {
            0 => source,
            1 => source.ToUpperInvariant(),
            _ => string.Create(source.Length, source, CopyWithCapitalizedFirst)
        };
    }

    private static void CopyWithCapitalizedFirst(Span<char> target, string source)
    {
        target[0] = char.ToUpperInvariant(source[0]);
        source.AsSpan(1).CopyTo(target.Slice(1));
    }
}