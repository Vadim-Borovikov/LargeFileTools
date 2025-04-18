using Generator.TextProviders;

namespace Generator.Tests;

public sealed class FileGeneratorTestsHelper
{
    internal readonly FileGenerator Generator;

    public FileGeneratorTestsHelper()
    {
        PoolTextProvider? provider = PoolTextProvider.TryCreateFrom(PoolFilePath, out string? error);
        Assert.IsNotNull(provider);
        Assert.IsNull(error);

        Generator = new FileGenerator(provider, LineFormat, MemoryUsageMegabytesPerWorker);
    }

    public void CreateFileAndCheckSize(long size)
    {
        File.Delete(OutputFilePath);
        Generator.Generate(size, OutputFilePath);
        FileInfo info = new(OutputFilePath);
        Assert.AreEqual(size, info.Length);
    }

    private static readonly string PoolFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
        "Generator", "bin", "Debug", "net9.0", "text pool.txt");

    public const string OutputFilePath = "text.txt";

    private const ushort MemoryUsageMegabytesPerWorker = 1;
    private const string LineFormat = "{0}. {1}";
}