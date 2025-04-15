namespace Generator;

internal static class RandomHelper
{
    public static T PickFrom<T>(this Random random, IReadOnlyList<T> list) => list[random.Next(0, list.Count)];

    public static int GetIntWithDigits(this Random random, byte digits)
    {
        return digits switch
        {
            1  => random.Next(1, 10),
            2  => random.Next(10, 100),
            3  => random.Next(100, 1000),
            4  => random.Next(1000, 10_000),
            5  => random.Next(10_000, 100_000),
            6  => random.Next(100_000, 1_000_000),
            7  => random.Next(1_000_000, 10_000_000),
            8  => random.Next(10_000_000, 100_000_000),
            9  => random.Next(100_000_000, 1_000_000_000),
            10 => random.Next(1_000_000_000, int.MaxValue),
            _  => throw new ArgumentOutOfRangeException(nameof(digits), "1–10 digits only")
        };
    }
}