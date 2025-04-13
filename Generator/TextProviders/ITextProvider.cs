namespace Generator.TextProviders;

internal interface ITextProvider
{
    public IEnumerable<int> GetLengths();
    string GetText(int length);
}