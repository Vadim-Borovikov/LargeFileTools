using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Sorter.Config;

internal static class Provider
{
    public static readonly Config Config;

    static Provider()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                                     .AddJsonFile("appsettings.json", false, true)
                                                                     .Build();

        Config = new Config();
        configuration.Bind(Config);

        ValidationContext context = new(Config);
        Validator.ValidateObject(Config, context, true);
    }
}