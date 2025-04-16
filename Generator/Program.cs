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
            Console.WriteLine("Usage: Generator.exe <output file path> <file size in bytes>");
            return;
        }

        string outputFilePath = args[0];

        if (!long.TryParse(args[1], out long fileSize) || (fileSize <= 0))
        {
            Console.WriteLine("Error: Invalid file size.");
            return;
        }

        PoolTextProvider? provider = PoolTextProvider.TryCreateFrom(config.PoolFilePath, out string? error);
        if (provider is null)
        {
            Console.WriteLine($"Error: {error}");
            return;
        }

        FileGenerator generator = new(provider, config.LineFormat, config.MemoryUsageMegaBytesPerWorker);
        bool success = generator.TryGenerate(fileSize, outputFilePath, out error);

        Console.WriteLine(success ? "Done." : $"Error: {error}");
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