using Generator.TextProviders;
using System.Collections.Concurrent;

namespace Generator;

internal sealed class FileGenerator
{
    public FileGenerator(ITextProvider textProvider, string lineFormat, byte workers, ushort memoryUsageMegaBytes)
    {
        _textProvider = textProvider;
        _lineFormat = lineFormat;
        _workers = workers;
        _memoryUsageMegaBytes = memoryUsageMegaBytes;
    }

    public string? TryGenerate(long fileSize, string outputFilePath)
    {
        HashSet<int> lengths = new(_textProvider.GetLengths());
        LineSizing? sizing = LineSizing.TryCreate(lengths, _lineFormat, out string? error);
        if (sizing is null)
        {
            return error ?? "";
        }

        string? lines = TryGenerateShort(fileSize, sizing.Value, out error);
        if (!string.IsNullOrWhiteSpace(error))
        {
            return error;
        }

        if (!string.IsNullOrWhiteSpace(lines))
        {
            File.WriteAllText(outputFilePath, lines);
            return null;
        }

        string duplicates = GenerateInitialDuplicateLines(sizing.Value);
        File.WriteAllText(outputFilePath, duplicates);
        int bytesWritten = duplicates.Length;

        int excessSize = (int) ((fileSize - bytesWritten) % sizing.Value.DefaultAdditionSize);
        if (excessSize > 0)
        {
            if (excessSize < (sizing.Value.MinLineLength + LineSizing.LineBreakLength))
            {
                excessSize += sizing.Value.DefaultAdditionSize;
            }

            string? excessLine = TryGenerateExcessLine(excessSize, sizing.Value, lengths);
            if (excessLine is null)
            {
                return $"Unable to create line of {excessSize} bytes.";
            }
            File.AppendAllText(outputFilePath, excessLine);
            bytesWritten += excessLine.Length;
        }

        long linesToWrite = (fileSize - bytesWritten) / sizing.Value.DefaultAdditionSize;
        long fullLinesWritten = 0;

        long memoryPerWorker = (long) _memoryUsageMegaBytes * 1024 * 1024 / _workers;
        int linesPerWorker = (int) memoryPerWorker / sizing.Value.DefaultAdditionSize;

        while (fullLinesWritten < linesToWrite)
        {
            long linesNeeded = Math.Min(linesPerWorker * _workers, linesToWrite - fullLinesWritten);
            long workersNeeded = linesNeeded / linesPerWorker;
            ConcurrentBag<string> results = new();
            Parallel.For(0, workersNeeded,
                _ => GenerateAndCollect(sizing.Value.DefaultLineContextLength, linesPerWorker, results));

            int excessLines = (int) linesNeeded % linesPerWorker;
            if (excessLines > 0)
            {
                // One last batch
                GenerateAndCollect(sizing.Value.DefaultLineContextLength, excessLines, results);
            }

            File.AppendAllText(outputFilePath, string.Join(string.Empty, results));

            fullLinesWritten += linesNeeded;
        }

        return null;
    }

    private string? TryGenerateShort(long fileSize, LineSizing sizing, out string? error)
    {
        error = null;

        if (fileSize < (2 * sizing.MinLineLength + LineSizing.LineBreakLength))
        {
            error = $"Unable to create 2 lines of {sizing.MinLineLength}-{sizing.MaxLineLength} with {fileSize} bytes.";
            return null;
        }

        if (fileSize < (3 * sizing.MinLineLength + 2 * LineSizing.LineBreakLength))
        {
            // can't fit 3 lines

            if (fileSize > (2 * sizing.MaxLineLength + LineSizing.LineBreakLength))
            {
                error =
                    $"Invalid pool arrangement: {fileSize} (fileSize) > (2 * {sizing.MaxLineLength} maxLineLength + {LineSizing.LineBreakLength} lineBreakLength)";
                return null;
            }

            ushort bothLinesLength = (ushort) (fileSize - LineSizing.LineBreakLength);
            int textLength =
                Math.Min(sizing.MaxLineLength,
                    bothLinesLength / 2 - LineSizing.MinNumberLength - sizing.LineDecorationLength);
            string duplicate = _textProvider.GetText(textLength);

            byte firstNumberLength = (byte) (bothLinesLength / 2 - textLength - sizing.LineDecorationLength);
            int firstNumber = RandomHelper.GetRandomIntWithDigits(firstNumberLength);
            string firstLine = GenerateLine(firstNumber.ToString(), duplicate, false);

            byte secondNumberLength =
                (byte) (bothLinesLength - firstLine.Length - textLength - sizing.LineDecorationLength);
            int secondNumber = RandomHelper.GetRandomIntWithDigits(secondNumberLength);
            string secondLine = GenerateLine(secondNumber.ToString(), duplicate, true);

            return firstLine + secondLine;
        }

        return null;
    }

    private string GenerateInitialDuplicateLines(LineSizing sizing)
    {
        string duplicate = _textProvider.GetText(sizing.MinTextLength);

        int firstNumber = RandomHelper.GetRandomIntWithDigits(1);
        string firstLine = GenerateLine(firstNumber.ToString(), duplicate, false);

        int secondNumber = RandomHelper.GetRandomIntWithDigits(1);
        string secondLine = GenerateLine(secondNumber.ToString(), duplicate, true);

        return firstLine + secondLine;
    }

    private string? TryGenerateExcessLine(int excessSize, LineSizing sizing, IReadOnlySet<int> lengths)
    {
        for (byte digits = LineSizing.MinNumberLength; digits <= LineSizing.MaxNumberLength; ++digits)
        {
            int textLength = excessSize - digits - sizing.LineDecorationLength - LineSizing.LineBreakLength;
            if (!lengths.Contains(textLength))
            {
                continue;
            }
            string text = _textProvider.GetText(textLength);
            int number = RandomHelper.GetRandomIntWithDigits(digits);
            return GenerateLine(number.ToString(), text, true);
        }

        return null;
    }

    private string GenerateLine(string number, string text, bool startWithNewLine)
    {
        return (startWithNewLine ? Environment.NewLine : string.Empty) + string.Format(_lineFormat, number, text);
    }

    private IEnumerable<string> GenerateLines(int defaultLineContextLength, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            byte digits = (byte) Random.Shared.Next(1, LineSizing.MaxNumberLength + 1);
            string number = RandomHelper.GetRandomIntWithDigits(digits).ToString();
            int textLength = defaultLineContextLength - number.Length;
            string text = _textProvider.GetText(textLength);
            yield return GenerateLine(number, text, true);
        }
    }

    private void GenerateAndCollect(int defaultLineContextLength, int lines, ConcurrentBag<string> results)
    {
        foreach (string line in GenerateLines(defaultLineContextLength, lines))
        {
            results.Add(line);
        }
    }

    private readonly ITextProvider _textProvider;
    private readonly byte _workers;
    private readonly uint _memoryUsageMegaBytes;
    private readonly string _lineFormat;
}