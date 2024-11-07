using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;

namespace gView.Server.AppCode.Configuration;

static internal class ConfigurationExtensions
{
    private static ConcurrentBag<IConfigParser> _parsers = new ConcurrentBag<IConfigParser>();

    // old config versions has keys like
    // services-folder => ServicesFolder
    // ...
    static public string Value(this IConfiguration configuration, string key)
        => configuration[key.Replace("-", "")]
        ?? configuration[key];

    static public IConfigurationSection Section(this IConfiguration configuration, string key)
        => configuration.GetSection(key.Replace("-", "")).Exists()
        ? configuration.GetSection(key.Replace("-", ""))
        : configuration.GetSection(key);

    public static void AddParser(this IConfigParser configParser)
    {
        _parsers.Add(configParser);
    }

    public static string GetParsedValue(this IConfiguration config, string key)
    {
        string val = config.Value(key);

        if (!String.IsNullOrEmpty(val))
        {
            foreach (var parser in _parsers)
            {
                val = parser.Parse(val);
            }
        }

        return val;
    }

    public static IConfiguration BuildConfigParsers(this IConfiguration configuration)
    {
        return new ConfigurationParserBuilder().Build(configuration);
    }
}
