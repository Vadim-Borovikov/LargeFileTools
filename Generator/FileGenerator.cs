using Generator.TextProviders;
using System.Text;

namespace Generator;

internal sealed class FileGenerator
{
    public FileGenerator(ITextProvider textProvider, string lineFormat, ushort memoryUsageMegabytesPerWorker)
    {
        _textProvider = textProvider;
        _lineFormat = lineFormat;
        _sizing = new LineSizing(textProvider, lineFormat);
        _memoryUsageBytesPerWorker = memoryUsageMegabytesPerWorker * 1024 * 1024;
    }

    public void Generate(long fileSize, string outputFilePath)
    {
        string initial = GenerateInitialContent(fileSize, _sizing);

        using (StreamWriter writer = new(outputFilePath))
        {
            writer.Write(initial);

            long bytesWritten = initial.Length;

            long writingLimit = fileSize - _sizing.MinLineLength - _sizing.MaxLineLength;

            while (bytesWritten < writingLimit)
            {
                string chunk = GenerateChunk(writingLimit - bytesWritten);
                if ((chunk.Length == 0) || ((bytesWritten + chunk.Length) >= writingLimit))
                {
                    break;
                }

                writer.Write(chunk);
                bytesWritten += chunk.Length;
            }

            while (bytesWritten < writingLimit)
            {
                string line = GenerateLine();
                writer.Write(line);
                bytesWritten += line.Length;
            }

            long bytesToAdd = fileSize - bytesWritten;
            if (bytesToAdd > 0)
            {
                string excess = GenerateExcessContent((int) bytesToAdd);
                writer.Write(excess);
            }
        }
    }

    private string GenerateChunk(long remainingBytes)
    {
        int workers = Environment.ProcessorCount;

        StringBuilder[] buffers = new StringBuilder[workers];
        for (int i = 0; i < workers; ++i)
        {
            buffers[i] = new StringBuilder(_memoryUsageBytesPerWorker);
        }

        Parallel.For(0, workers, i => FillBuffer(buffers[i]));

        StringBuilder result = new(workers * _memoryUsageBytesPerWorker);
        // ReSharper disable once LoopCanBePartlyConvertedToQuery
        foreach (StringBuilder buffer in buffers)
        {
            if ((result.Length + buffer.Length) <= remainingBytes)
            {
                result.Append(buffer);
            }
        }
        return result.ToString();
    }

    private void FillBuffer(StringBuilder buffer)
    {
        int bytesFilled = 0;

        while (bytesFilled < _memoryUsageBytesPerWorker)
        {
            string line = GenerateLine();

            if (line.Length > (_memoryUsageBytesPerWorker - bytesFilled))
            {
                break;
            }

            buffer.Append(line);
            bytesFilled += line.Length;
        }
    }

    private string GenerateInitialContent(long fileSize, LineSizing sizing)
    {
        if (fileSize < (3 * sizing.MinLineLength))
        {
            // can't fit 3 lines
            return GenerateShortContent((int) fileSize);
        }

        long minLines = (fileSize - sizing.MaxLineLength) / sizing.MaxLineLength;
        bool areDuplicatesGaranteed = minLines > _textProvider.TextsAmount;
        return areDuplicatesGaranteed ? GenerateLine() : GenerateInitialDuplicateLines();
    }

    private string GenerateShortContent(int length)
    {
        if (length < (2 * _sizing.MinLineLength))
        {
            throw new InvalidOperationException($"Unable to create 2 lines of {_sizing.MinLineLength}-{_sizing.MaxLineLength} with {length} bytes.");
        }

        int firstLineLength = length / 2;

        byte firstNumberLength = GetNumberMinLength(firstLineLength);

        int textLength = firstLineLength - firstNumberLength - _sizing.LineDecorationLength;
        string duplicate = _textProvider.GetText(textLength);

        string firstLine = GenerateLine(firstNumberLength, duplicate);

        int secondLineLength = length - firstLine.Length;

        byte secondNumberLength = (byte) (secondLineLength - textLength - _sizing.LineDecorationLength);

        string secondLine = GenerateLine(secondNumberLength, duplicate);

        return firstLine + secondLine;
    }

    private string GenerateInitialDuplicateLines()
    {
        string duplicate = _textProvider.GetText(_sizing.MinTextLength);

        string firstLine = GenerateLine(LineSizing.MinNumberLength, duplicate);
        string secondLine = GenerateLine(LineSizing.MinNumberLength, duplicate);

        return firstLine + secondLine;
    }

    private string GenerateExcessContent(int length)
    {
        if (length <= _sizing.MaxLineLength)
        {
            return GenerateLine(length);
        }

        string firstLine = GenerateLine(length / 2);
        string secondLine = GenerateLine(length - firstLine.Length);
        return firstLine + secondLine;
    }

    private byte GetNumberMinLength(int lineLength)
    {
        return (byte) Math.Max(LineSizing.MinNumberLength,
            lineLength - _sizing.LineDecorationLength - _sizing.MaxTextLength);
    }

    private string GenerateLine(int length)
    {
        byte numberLength = GetNumberMinLength(length);
        int textLength = length - numberLength - _sizing.LineDecorationLength;
        string text = _textProvider.GetText(textLength);
        return GenerateLine(numberLength, text);
    }

    private string GenerateLine(byte? numberLength = null, string? text = null)
    {
        numberLength ??= (byte) Random.Shared.Next(LineSizing.MinNumberLength, LineSizing.MaxNumberLength + 1);
        int number = Random.Shared.GetIntWithDigits(numberLength.Value);

        text ??= _textProvider.GetText();

        return string.Format(_lineFormat, number, text) + Environment.NewLine;
    }

    private readonly ITextProvider _textProvider;
    private readonly int _memoryUsageBytesPerWorker;
    private readonly string _lineFormat;
    private readonly LineSizing _sizing;
}