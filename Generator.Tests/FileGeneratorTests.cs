using System.Diagnostics;
using Generator.TextProviders;

namespace Generator.Tests;

[TestClass]
public class FileGeneratorTests
{
    [TestInitialize]
    public void Initialize()
    {
        PoolTextProvider provider = new(PoolFilePath);
        _generator ??= new FileGenerator(provider, LineFormat, Workers, MemoryUsageMegabytes);
    }

    [TestMethod]
    public void TooLittleSizeTests()
    {
        Assert.IsNotNull(_generator);
        for (byte b = 0; b < MinFileSize; ++b)
        {
            CheckSizeIsTooLittle(_generator, b);
        }
    }

    [TestMethod]
    public void NotTooLittleSizeTests()
    {
        Assert.IsNotNull(_generator);
        for (int i = MinFileSize + 1; i < (10 * MinFileSize); ++i)
        {
            CreateAndCheckFile(_generator, i);
        }
    }

    [TestMethod]
    public void TensSizeTest()
    {
        Assert.IsNotNull(_generator);
        int maxSize = (int) Math.Pow(10, 9);
        for (int size = 10; size <= maxSize; size *= 10)
        {
            CreateAndCheckFile(_generator, size);
        }
    }

    private static void CheckSizeIsTooLittle(FileGenerator generator, byte size)
    {
        string? result = generator.TryGenerate(size, OutputFilePath);
        Assert.AreEqual(result, $"Unable to create 2 lines of {MinLineLength}-{MaxLineLength} with {size} bytes.");
    }

    private static void CreateAndCheckFile(FileGenerator generator, int size)
    {
        File.Delete(OutputFilePath);
        string? result = generator.TryGenerate(size, OutputFilePath);
        Assert.IsNull(result);
        FileInfo info = new(OutputFilePath);
        if (size != info.Length)
        {
            Debugger.Break();
        }
        Assert.AreEqual(size, info.Length);

        string decoration = string.Format(LineFormat, "", "");

        using (StreamReader reader = new(OutputFilePath))
        {
            string? line;
            string? firstText = null;
            string? secondText = null;
            while ((line = reader.ReadLine()) is not null)
            {
                string text = ChechLineAndGetText(line, decoration);

                if (firstText is null)
                {
                    firstText = text;
                    continue;
                }
                secondText ??= text;
            }
            Assert.IsNotNull(firstText);
            Assert.AreEqual(firstText, secondText);
        }
    }

    private static string ChechLineAndGetText(string line, string decoration)
    {
        int decorationIndex = line.IndexOf(decoration, StringComparison.Ordinal);
        Assert.IsTrue(decorationIndex > 0);

        string numberPart = line.Substring(0, decorationIndex);
        Assert.IsTrue(int.TryParse(numberPart, out int number));
        Assert.IsTrue(number > 0);

        int textIndex = numberPart.Length + decoration.Length;
        Assert.IsTrue(line.Length > textIndex);
        return line.Substring(textIndex);
    }

    private FileGenerator? _generator;

    private static readonly string PoolFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
        "Generator", "bin", "Debug", "net9.0", "text pool.txt");

    private const byte Workers = 8;
    private const ushort MemoryUsageMegabytes = 2048;
    private const string LineFormat = "{0}. {1}";
    private const string OutputFilePath = "text.txt";
    private const byte MinLineLength = 4;
    private const byte MaxLineLength = 22;
    private const byte MinFileSize = 10;
}