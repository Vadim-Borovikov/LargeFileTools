namespace Generator.TextProviders;

internal interface ITextProvider
{
    public const byte MinLength = 1;
    public int MaxLength { get; }
    public int TextsAmount { get; }
    string GetText();
    string GetText(int length);
}