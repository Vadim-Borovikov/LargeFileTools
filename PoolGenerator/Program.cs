namespace PoolGenerator;

internal static class Program
{
    private static void Main()
    {
        DatamuseProvider? provider = DatamuseProvider.TryCreate(WordsMeaning, MaxRequestSize);
        if (provider is null)
        {
            Console.Error.WriteLine("Unable to create provider.");
            return;
        }

        IEnumerable<string> texts = provider.GetDistinctStrings()
                                            .Where(IsProper)
                                            .Select(StringExtensions.Capitalize);
        File.WriteAllText(FilePath, string.Join(Environment.NewLine, texts));
        Console.WriteLine("Done.");
    }

    private static bool IsProper(string s)
    {
        if (s.Length == 0)
        {
            return false;
        }

        char first = s[0];
        char last = s[s.Length - 1];

        if (first is < 'a' or > 'z' || last is < 'a' or > 'z')
        {
            return false;
        }

        foreach (char c in s)
        {
            if (c is (< 'a' or > 'z') and not ' ' and not '-')
            {
                return false;
            }
        }

        return true;
    }

    private const string WordsMeaning = "thing";
    private const ushort MaxRequestSize = 1000;
    private const string FilePath = "text pool.txt";
}
