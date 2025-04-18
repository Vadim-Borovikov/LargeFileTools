namespace Sorter;

internal sealed class LineSorter
{
    public LineSorter(int linesPerChunk, ushort maxTempFiles, string tempFolderPath)
    {
        _linesPerChunk = linesPerChunk;
        _maxTempFiles = maxTempFiles;
        _tempFolderPath = tempFolderPath;
    }

    public async Task SortAsync(string inputFilePath, string outputFilePath)
    {
        if (!Directory.Exists(_tempFolderPath))
        {
            Directory.CreateDirectory(_tempFolderPath);
        }

        List<Task<string>> sortTasks = new(_maxTempFiles);
        string[] filesToMerge;
        List<string> mergedFiles = new(_maxTempFiles);

        await foreach (List<string> chunk in ReadChunksAsync(inputFilePath))
        {
            Task<string> task = Task.Run(() => ParseSortAndWriteAsync(chunk));
            sortTasks.Add(task);

            bool readyToMerge = (sortTasks.Count + mergedFiles.Count) == (_maxTempFiles - 1);
            if (!readyToMerge)
            {
                continue;
            }

            filesToMerge = await Task.WhenAll(sortTasks);
            sortTasks.Clear();

            string mergedFile =
                filesToMerge.Length > 1 ? await MergeAndDeleteAsync(filesToMerge) : filesToMerge.Single();
            mergedFiles.Add(mergedFile);

            if (mergedFiles.Count < (_maxTempFiles - 1))
            {
                continue;
            }

            mergedFile = await MergeAndDeleteAsync(mergedFiles);
            mergedFiles.Clear();
            mergedFiles.Add(mergedFile);
        }

        if (sortTasks.Count > 0)
        {
            filesToMerge = await Task.WhenAll(sortTasks);
            mergedFiles.AddRange(filesToMerge);
        }

        await MergeToAndDeleteAsync(mergedFiles, outputFilePath, true);
    }

    private async IAsyncEnumerable<List<string>> ReadChunksAsync(string inputFilePath)
    {
        using (StreamReader reader = new(inputFilePath))
        {
            while (!reader.EndOfStream)
            {
                List<string> chunk = new(_linesPerChunk);

                for (int i = 0; (i < _linesPerChunk) && !reader.EndOfStream; ++i)
                {
                    string? line = await reader.ReadLineAsync();
                    if (line is not null)
                    {
                        chunk.Add(line);
                    }
                }

                yield return chunk;
            }
        }
    }

    private async Task<string> ParseSortAndWriteAsync(IEnumerable<string> raw)
    {
        IEnumerable<string> toWrite = raw.Select(Line.Parse)
                                         .Order()
                                         .Select(l => l.ToString());
        string tempFile = Path.Combine(_tempFolderPath, Path.GetRandomFileName());
        await File.WriteAllLinesAsync(tempFile, toWrite);
        return tempFile;
    }

    private async Task<string> MergeAndDeleteAsync(ICollection<string> files)
    {
        if (files.Count == 1)
        {
            return files.Single();
        }

        string result = Path.Combine(_tempFolderPath, Path.GetRandomFileName());
        await MergeToAndDeleteAsync(files, result, false);
        return result;
    }

    private async Task MergeToAndDeleteAsync(ICollection<string> files, string result, bool final)
    {
        if (files.Count == 1)
        {
            File.Move(files.Single(), result, true);
            return;
        }

        await MergeToAsync(files, result);
        Delete(files, final);
    }

    private static async Task MergeToAsync(IEnumerable<string> files, string result)
    {
        List<StreamReader> readers = files.Select(p => new StreamReader(p)).ToList();

        try
        {
            PriorityQueue<FileLine, Line> minHeap = new();

            foreach (StreamReader reader in readers)
            {
                FileLine? fl = await ReadAndParseFromAsync(reader);
                if (fl is not null)
                {
                    minHeap.Enqueue(fl, fl.Line);
                }
            }

            await using (StreamWriter writer = new(result))
            {
                while (minHeap.Count > 0)
                {
                    FileLine smallest = minHeap.Dequeue();
                    await writer.WriteLineAsync(smallest.Line.ToString());

                    FileLine? next = await ReadAndParseFromAsync(smallest.Reader);
                    if (next is not null)
                    {
                        minHeap.Enqueue(next, next.Line);
                    }
                }
            }
        }
        finally
        {
            foreach (StreamReader reader in readers)
            {
                reader.Dispose();
            }
        }
    }

    private static async Task<FileLine?> ReadAndParseFromAsync(StreamReader reader)
    {
        string? raw = await reader.ReadLineAsync();
        if (raw is null)
        {
            return null;
        }

        Line line = Line.Parse(raw);
        return new FileLine(reader, line);
    }

    private void Delete(IEnumerable<string> files, bool final)
    {
        if (final)
        {
            Directory.Delete(_tempFolderPath, true);
            return;
        }

        foreach (string file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // ignore
            }
        }
    }

    private readonly int _linesPerChunk;
    private readonly ushort _maxTempFiles;
    private readonly string _tempFolderPath;
}
