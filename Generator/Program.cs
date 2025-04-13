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
            Console.WriteLine("Usage: LargeFileTools.Generator.exe <output file path> <file size in bytes>");
            return;
        }

        string outputFilePath = args[0];

        if (!long.TryParse(args[1], out long fileSize) || (fileSize <= 0))
        {
            Console.WriteLine("Error: Invalid file size.");
            return;
        }

        PoolTextProvider provider = new(config.PoolFilePath);

        FileGenerator generator = new(provider, config.LineFormat, config.Workers, config.MemoryUsageMegaBytes);
        string? errorMessage = generator.TryGenerate(fileSize, outputFilePath);

        Console.WriteLine(string.IsNullOrWhiteSpace(errorMessage) ? "Done." : $"Error: {errorMessage}");
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