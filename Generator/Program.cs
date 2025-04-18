using System.ComponentModel.DataAnnotations;
using Generator.TextProviders;
using Microsoft.Extensions.Configuration;

namespace Generator;

internal static class Program
{
    private static void Main(string[] args)
    {
        Config config = GetConfig();

        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: Generator.exe <output file path> <file size in bytes>");
            return;
        }

        string outputFilePath = Path.GetFullPath(args[0]);

        if (!long.TryParse(args[1], out long fileSize) || (fileSize <= 0))
        {
            Console.Error.WriteLine("Invalid file size.");
            return;
        }

        PoolTextProvider? provider = PoolTextProvider.TryCreateFrom(config.PoolFilePath, out string? error);
        if (provider is null)
        {
            Console.Error.WriteLine(error);
            return;
        }

        FileGenerator generator = new(provider, config.LineFormat, config.MemoryUsageMegaBytesPerWorker);
        try
        {
            Console.Write($"Generating file of {fileSize:N0} bytes from pool at {Path.GetFullPath(config.PoolFilePath)}...");
            generator.Generate(fileSize, outputFilePath);
            Console.WriteLine(" done.");
            Console.WriteLine($"You may see result in {outputFilePath}.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    private static Config GetConfig()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                                     .AddJsonFile("appsettings.json", false, true)
                                                                     .Build();

        Config config = new();
        configuration.Bind(config);

        ValidationContext context = new(config);
        Validator.ValidateObject(config, context, true);

        return config;
    }
}