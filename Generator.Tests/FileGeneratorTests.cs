using Sorter;

namespace Generator.Tests;

[TestClass]
public class FileGeneratorTests
{
    [TestInitialize]
    public virtual void Initialize() => _helper ??= new FileGeneratorTestsHelper();

    [TestMethod]
    public void Test01_TooLittleSize()
    {
        Assert.IsNotNull(_helper);
        for (byte b = 0; b < MinFileSize; ++b)
        {
            CheckSizeIsTooLittle(_helper.Generator, b);
        }
    }

    [TestMethod]
    public void Test02_NotTooLittleSize()
    {
        for (int i = MinFileSize + 1; i < (10 * MinFileSize); ++i)
        {
            CreateAndCheckFile(i);
        }
    }

    [TestMethod]
    public void Test03_10() => CreateAndCheckFile(10);

    [TestMethod]
    public void Test04_100() => CreateAndCheckFile(100);

    [TestMethod]
    public void Test05_1000() => CreateAndCheckFile(1000);

    [TestMethod]
    public void Test06_10_000() => CreateAndCheckFile(10_000);

    [TestMethod]
    public void Test07_100_000() => CreateAndCheckFile(100_000);

    [TestMethod]
    public void Test08_1_000_000() => CreateAndCheckFile(1_000_000);

    [TestMethod]
    public void Test09_10_000_000() => CreateAndCheckFile(10_000_000);

    [TestMethod]
    public void Test10_100_000_000() => CreateAndCheckFile(100_000_000);

    [TestMethod]
    public void Test11_1_000_000_000() => CreateAndCheckFile(1_000_000_000);

    private static void CheckSizeIsTooLittle(FileGenerator generator, byte size)
    {
        try
        {
            generator.Generate(size, FileGeneratorTestsHelper.OutputFilePath);
            Assert.Fail();
        }
        catch (Exception ex)
        {
            Assert.AreEqual($"Unable to create 2 lines of {MinLineLength}-{MaxLineLength} with {size} bytes.",
                ex.Message);
        }
    }

    private void CreateAndCheckFile(long size)
    {
        Assert.IsNotNull(_helper);
        CreateAndCheckFile(_helper, size);
    }

    private static void CreateAndCheckFile(FileGeneratorTestsHelper helper, long size)
    {
        helper.CreateFileAndCheckSize(size);

        HashSet<string> texts = new();
        bool duplicateFound = false;
        using (StreamReader reader = new(FileGeneratorTestsHelper.OutputFilePath))
        {
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                Line? parsed = Line.TryParse(line);
                Assert.IsNotNull(parsed);
                if (!duplicateFound)
                {
                    if (texts.Contains(parsed.Text))
                    {
                        duplicateFound = true;
                    }
                    else
                    {
                        texts.Add(parsed.Text);
                    }
                }
            }
            Assert.IsTrue(duplicateFound);
        }
    }

    private FileGeneratorTestsHelper? _helper;

    private const byte MinLineLength = 4;
    private const byte MaxLineLength = 47;
    private const byte MinFileSize = 10;
}