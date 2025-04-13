﻿namespace PoolGenerator;

internal static class Program
{
    private static void Main()
    {
        DatamuseProvider? provider = DatamuseProvider.TryCreate(WordsMeaning, MaxRequestSize);
        if (provider is null)
        {
            Console.WriteLine("Unable to create provider.");
            return;
        }

        IEnumerable<string> strings = provider.GetDistinctStrings()
                                              .Where(IsProper)
                                              .Select(CapitalizeFirst)
                                              .OrderBy(s => s.Length);
        File.WriteAllText(FilePath, string.Join(Environment.NewLine, strings));
    }

    private static bool IsProper(string s)
    {
        if (s.Length is 0 or > MaxLength)
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

    private static string CapitalizeFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        if (char.IsUpper(s[0]))
        {
            return s;
        }

        return char.ToUpperInvariant(s[0]) + s.Substring(1);
    }

    private const string WordsMeaning = "thing";
    private const ushort MaxRequestSize = 1000;
    private const string FilePath = "text pool.txt";
    private const byte MaxLength = 10;
}
