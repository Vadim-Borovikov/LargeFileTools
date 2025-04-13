namespace Generator;

internal static class RandomHelper
{
    public static T GetRandomElement<T>(this IReadOnlyList<T> list) => list[Random.Shared.Next(0, list.Count)];

    public static int GetRandomIntWithDigits(byte digits)
    {
        return digits switch
        {
            1 => Random.Shared.Next(1, 10),
            2 => Random.Shared.Next(10, 100),
            3 => Random.Shared.Next(100, 1000),
            4 => Random.Shared.Next(1000, 10_000),
            5 => Random.Shared.Next(10_000, 100_000),
            6 => Random.Shared.Next(100_000, 1_000_000),
            7 => Random.Shared.Next(1_000_000, 10_000_000),
            8 => Random.Shared.Next(10_000_000, 100_000_000),
            9 => Random.Shared.Next(100_000_000, 1_000_000_000),
            10 => Random.Shared.Next(1_000_000_000),
            _ => throw new ArgumentOutOfRangeException(nameof(digits), "1–10 digits only")
        };
    }
}