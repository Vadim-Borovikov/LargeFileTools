using Generator.TextProviders;

namespace Generator.Tests;

[TestClass]
public class FileGeneratorTests
{
    [TestInitialize]
    public void Initialize()
    {
        PoolTextProvider? provider = PoolTextProvider.TryCreateFrom(PoolFilePath, out string? error);
        Assert.IsNotNull(provider);
        Assert.IsNull(error);

        _generator ??= new FileGenerator(provider, LineFormat, MemoryUsageMegabytesPerWorker);
    }

    [TestMethod]
    public void Test01_TooLittleSize()
    {
        Assert.IsNotNull(_generator);
        for (byte b = 0; b < MinFileSize; ++b)
        {
            CheckSizeIsTooLittle(_generator, b);
        }
    }

    [TestMethod]
    public void Test02_NotTooLittleSize()
    {
        Assert.IsNotNull(_generator);
        for (int i = MinFileSize + 1; i < (10 * MinFileSize); ++i)
        {
            CreateAndCheckFile(_generator, i);
        }
    }

    [TestMethod]
    public void Test03_10()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 10);
    }

    [TestMethod]
    public void Test04_100()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 100);
    }

    [TestMethod]
    public void Test05_1000()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 1000);
    }

    [TestMethod]
    public void Test06_10_000()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 10_000);
    }

    [TestMethod]
    public void Test07_100_000()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 100_000);
    }

    [TestMethod]
    public void Test08_1_000_000()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 1_000_000);
    }

    [TestMethod]
    public void Test09_10_000_000()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 10_000_000);
    }

    [TestMethod]
    public void Test10_100_000_000()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 100_000_000);
    }

    [TestMethod]
    public void Test11_1_000_000_000()
    {
        Assert.IsNotNull(_generator);
        CreateAndCheckFile(_generator, 1_000_000_000);
    }

    private static void CheckSizeIsTooLittle(FileGenerator generator, byte size)
    {
        try
        {
            generator.Generate(size, OutputFilePath);
            Assert.Fail();
        }
        catch (Exception ex)
        {
            Assert.AreEqual($"Unable to create 2 lines of {MinLineLength}-{MaxLineLength} with {size} bytes.",
                ex.Message);
        }
    }

    private static void CreateAndCheckFile(FileGenerator generator, int size)
    {
        File.Delete(OutputFilePath);
        generator.Generate(size, OutputFilePath);
        FileInfo info = new(OutputFilePath);
        Assert.AreEqual(size, info.Length);

        string decoration = string.Format(LineFormat, "", "");

        HashSet<string> texts = new();
        bool duplicateFound = false;
        using (StreamReader reader = new(OutputFilePath))
        {
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                string text = CheckLineAndGetText(line, decoration);
                if (!duplicateFound)
                {
                    if (texts.Contains(text))
                    {
                        duplicateFound = true;
                    }
                    else
                    {
                        texts.Add(text);
                    }
                }
            }
            Assert.IsTrue(duplicateFound);
        }
    }

    private static string CheckLineAndGetText(string line, string decoration)
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

    private const ushort MemoryUsageMegabytesPerWorker = 1;
    private const string LineFormat = "{0}. {1}";
    private const string OutputFilePath = "text.txt";
    private const byte MinLineLength = 4;
    private const byte MaxLineLength = 47;
    private const byte MinFileSize = 10;
}