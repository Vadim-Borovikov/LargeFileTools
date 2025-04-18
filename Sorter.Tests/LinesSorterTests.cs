using Generator.Tests;

namespace Sorter.Tests;

[TestClass]
public class FileSorterTests
{
    [TestInitialize]
    public virtual void Initialize()
    {
        _generatorHelper ??= new FileGeneratorTestsHelper();
        _sorter = new LineSorter(LinesToSortAtOnce, MaxTempFiles, CustomTempFolderPath);
    }

    [TestMethod]
    public async Task Test01_ExactResult()
    {
        Assert.IsNotNull(_sorter);

        const string input = "test input.txt";
        const string outputExpected = "test output expected.txt";

        await _sorter.SortAsync(input, OutputSorterFilePath);
        string expected = await File.ReadAllTextAsync(outputExpected);
        string actual = await File.ReadAllTextAsync(OutputSorterFilePath);
        Assert.AreEqual(expected, actual);
        File.Delete(OutputSorterFilePath);
    }

    [TestMethod]
    public Task Test02_100() => CheckAsync(100);

    [TestMethod]
    public Task Test03_1000() => CheckAsync(1000);

    [TestMethod]
    public Task Test04_10_000() => CheckAsync(10_000);

    [TestMethod]
    public Task Test05_100_000() => CheckAsync(100_000);

    [TestMethod]
    public Task Test06_1_000_000() => CheckAsync(1_100_000);

    [TestMethod]
    public Task Test07_10_000_000() => CheckAsync(10_000_000);

    [TestMethod]
    public Task Test08_100_000_000() => CheckAsync(100_000_000);

    [TestMethod]
    public Task Test09_1_000_000_000() => CheckAsync(1_000_000_000);

    private async Task CheckAsync(int fileSize)
    {
        Assert.IsNotNull(_generatorHelper);
        Assert.IsNotNull(_sorter);

        _generatorHelper.CreateFileAndCheckSize(fileSize);

        await _sorter.SortAsync(FileGeneratorTestsHelper.OutputFilePath, OutputSorterFilePath);
        FileInfo sorted = new(OutputSorterFilePath);

        Assert.AreEqual(fileSize, sorted.Length);

        using (StreamReader reader = new(OutputSorterFilePath))
        {
            string? row = await reader.ReadLineAsync();
            Assert.IsNotNull(row);
            Line previous = Line.Parse(row);

            while (!reader.EndOfStream)
            {
                row = await reader.ReadLineAsync();
                Assert.IsNotNull(row);
                Line current = Line.Parse(row);
                Assert.IsTrue(previous.CompareTo(current) <= 0);
            }
        }
    }

    private FileGeneratorTestsHelper? _generatorHelper;

    private LineSorter? _sorter;

    private const string OutputSorterFilePath = "sorted.txt";

    private const ushort MaxTempFiles = 40;
    private const int LinesToSortAtOnce = 4000000;
    private const string CustomTempFolderPath = "temp";
}