using Sorter.Config;

namespace Sorter;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            await Console.Error.WriteLineAsync("Usage: sorter.exe <input file path> <output file path>");
            return;
        }

        string inputFilePath = Path.GetFullPath(args[0]);
        string outputFilePath = Path.GetFullPath(args[1]);

        LineSorter sorter =
            new(Provider.Config.LinesToSortAtOnce, Provider.Config.MaxTempFiles, Provider.Config.CustomTempFolderPath);
        try
        {
            Console.Write($"Sorting {inputFilePath}...");
            await sorter.SortAsync(inputFilePath, outputFilePath);
            Console.WriteLine(" done.");
            Console.WriteLine($"You may see result in {outputFilePath}.");
        }
        catch (AggregateException ex)
        {
            foreach (Exception inner in ex.Flatten().InnerExceptions)
            {
                await Console.Error.WriteLineAsync(inner.Message);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
}