namespace Sorter;

internal sealed class FileLine : IComparable<FileLine>
{
    public readonly StreamReader Reader;
    public readonly Line Line;

    public FileLine(StreamReader reader, Line line)
    {
        Reader = reader;
        Line = line;
    }

    public int CompareTo(FileLine? other) => Line.CompareTo(other?.Line);
}